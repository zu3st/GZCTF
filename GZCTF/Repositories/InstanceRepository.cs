using CTFServer.Models.Internal;
using CTFServer.Repositories.Interface;
using CTFServer.Services.Interface;
using CTFServer.Utils;
using Microsoft.EntityFrameworkCore;

namespace CTFServer.Repositories;

public class InstanceRepository : RepositoryBase, IInstanceRepository
{
    private readonly IContainerService service;
    private readonly IContainerRepository containerRepository;
    private readonly IGameEventRepository gameEventRepository;
    private readonly ILogger<InstanceRepository> logger;

    public InstanceRepository(AppDbContext _context,
        IContainerService _service,
        IContainerRepository _containerRepository,
        IGameEventRepository _gameEventRepository,
        ILogger<InstanceRepository> _logger) : base(_context)
    {
        logger = _logger;
        service = _service;
        gameEventRepository = _gameEventRepository;
        containerRepository = _containerRepository;
    }

    public async Task<Instance?> GetInstance(Participation part, int challengeId, CancellationToken token = default)
    {
        var instance = await context.Instances
            .Include(i => i.FlagContext)
            .Where(e => e.ChallengeId == challengeId && e.Participation == part)
            .SingleOrDefaultAsync(token);

        if (instance is null)
        {
            logger.SystemLog($"Team participation object is null, this might be unexpected [{part.Id}, {challengeId}]", TaskStatus.NotFound, LogLevel.Warning);
            return null;
        }

        if (instance.IsLoaded)
            return instance;

        var challenge = instance.Challenge;

        if (challenge is null || !challenge.IsEnabled)
            return null;

        if (challenge.Type.IsStatic())
        {
            instance.FlagContext = null; // Use challenge to verify
            instance.IsLoaded = true;

            await SaveAsync(token);
        }
        else
        {
            if (challenge.Type.IsAttachment())
            {
                bool saved = false;
                int retry = 0;
                do
                {
                    var flags = await context.FlagContexts
                        .Where(e => e.Challenge == challenge && !e.IsOccupied)
                        .ToListAsync(token);

                    if (flags.Count == 0)
                    {
                        logger.SystemLog($"The number of dynamic attachments requested by the challenge {challenge.Title}#{challenge.Id} is insufficient", TaskStatus.Fail, LogLevel.Warning);
                        return null;
                    }

                    var pos = Random.Shared.Next(flags.Count);
                    flags[pos].IsOccupied = true;

                    instance.FlagId = flags[pos].Id;

                    try
                    {
                        // FlagId need to be unique
                        await SaveAsync(token);
                        saved = true;
                    }
                    catch
                    {
                        retry++;
                        logger.SystemLog($"The dynamic attachment allocated to the challenge {challenge.Title}#{challenge.Id} failed to save, retrying: {retry} times", TaskStatus.Fail, LogLevel.Warning);
                        if (retry >= 3)
                            return null;
                        await Task.Delay(100, token);
                    }
                } while (!saved);
            }
            else
            {
                instance.FlagContext = new()
                {
                    Challenge = challenge,
                    // Tiny probability will produce the same FLAG,
                    // but this will not affect the correctness of the answer
                    Flag = challenge.GenerateFlag(part),
                    IsOccupied = true
                };
            }
        }

        return instance;
    }

    public async Task<bool> DestroyContainer(Container container, CancellationToken token = default)
    {
        try
        {
            await service.DestroyContainerAsync(container, token);
            await containerRepository.RemoveContainer(container, token);
            return true;
        }
        catch (Exception ex)
        {
            logger.SystemLog($"Destroying container [{container.ContainerId[..12]}] ({container.Image.Split("/").LastOrDefault()}): {ex.Message}", TaskStatus.Fail, LogLevel.Warning);
            return false;
        }
    }

    public async Task<TaskResult<Container>> CreateContainer(Instance instance, Team team, UserInfo user, int containerLimit = 3, CancellationToken token = default)
    {
        if (string.IsNullOrEmpty(instance.Challenge.ContainerImage) || instance.Challenge.ContainerExposePort is null)
        {
            logger.SystemLog($"Unable to start container instance for challenge {instance.Challenge.Title}", TaskStatus.Denied, LogLevel.Warning);
            return new TaskResult<Container>(TaskStatus.Fail);
        }

        if (await context.Instances.CountAsync(i => i.Participation == instance.Participation
                && i.Container != null
                && i.Container.Status == ContainerStatus.Running, token) >= containerLimit)
            return new TaskResult<Container>(TaskStatus.Denied);

        if (instance.Container is null)
        {
            await context.Entry(instance).Reference(e => e.FlagContext).LoadAsync(token);
            var container = await service.CreateContainerAsync(new ContainerConfig()
            {
                TeamId = team.Id.ToString(),
                UserId = user.Id,
                Flag = instance.FlagContext?.Flag, // Static challenge has no specific flag
                Image = instance.Challenge.ContainerImage,
                CPUCount = instance.Challenge.CPUCount ?? 1,
                MemoryLimit = instance.Challenge.MemoryLimit ?? 64,
                StorageLimit = instance.Challenge.StorageLimit ?? 256,
                PrivilegedContainer = instance.Challenge.PrivilegedContainer ?? false,
                ExposedPort = instance.Challenge.ContainerExposePort ?? throw new ArgumentException("Invalid port on creating container"),
            }, token);

            if (container is null)
            {
                logger.SystemLog($"Failed to start container instance for challenge {instance.Challenge.Title}", TaskStatus.Fail, LogLevel.Warning);
                return new TaskResult<Container>(TaskStatus.Fail);
            }

            instance.Container = container;
            instance.LastContainerOperation = DateTimeOffset.UtcNow;

            logger.Log($"Team {team.Name} started a container instance for challenge {instance.Challenge.Title} [{container.Id}]", user, TaskStatus.Success);

            // Will save instance together
            await gameEventRepository.AddEvent(new()
            {
                Type = EventType.ContainerStart,
                GameId = instance.Challenge.GameId,
                TeamId = instance.Participation.TeamId,
                UserId = user.Id,
                Content = $"Start container instance for {instance.Challenge.Title}#{instance.Challenge.Id}"
            }, token);
        }

        return new TaskResult<Container>(TaskStatus.Success, instance.Container);
    }

    public async Task ProlongContainer(Container container, TimeSpan time, CancellationToken token = default)
    {
        container.ExpectStopAt += time;
        await SaveAsync(token);
    }

    public Task<Instance[]> GetInstances(Challenge challenge, CancellationToken token = default)
        => context.Instances.Where(i => i.Challenge == challenge).OrderBy(i => i.ParticipationId)
            .Include(i => i.Participation).ThenInclude(i => i.Team).ToArrayAsync(token);

    public async Task<CheatCheckInfo> CheckCheat(Submission submission, CancellationToken token = default)
    {
        CheatCheckInfo checkInfo = new();

        var instances = await context.Instances.Where(i => i.ChallengeId == submission.ChallengeId &&
                i.ParticipationId != submission.ParticipationId)
                .Include(i => i.Challenge).Include(i => i.FlagContext)
                .Include(i => i.Participation).ThenInclude(i => i.Team).ToArrayAsync(token);

        foreach (var instance in instances)
        {
            if (instance.FlagContext?.Flag == submission.Answer)
            {
                checkInfo.AnswerResult = AnswerResult.CheatDetected;
                checkInfo.CheatUser = submission.User;
                checkInfo.CheatTeam = submission.Team;
                checkInfo.SourceTeam = instance.Participation.Team;
                checkInfo.Challenge = instance.Challenge;

                var updateSub = await context.Submissions.Where(s => s.Id == submission.Id).SingleAsync(token);

                if (updateSub is not null)
                    updateSub.Status = AnswerResult.CheatDetected;

                await SaveAsync(token);

                return checkInfo;
            }
        }

        return checkInfo;
    }

    public async Task<(SubmissionType, AnswerResult)> VerifyAnswer(Submission submission, CancellationToken token = default)
    {
        var instance = await context.Instances
            .IgnoreAutoIncludes()
            .Include(i => i.FlagContext)
            .SingleOrDefaultAsync(i => i.ChallengeId == submission.ChallengeId &&
                i.ParticipationId == submission.ParticipationId, token);

        // submission is from the queue, do not modify it directly
        // we need to requery the entity to ensure it is being tracked correctly
        var updateSub = await context.Submissions.SingleAsync(s => s.Id == submission.Id, token);

        var ret = SubmissionType.Unaccepted;

        if (instance is null)
        {
            submission.Status = AnswerResult.NotFound;
            return (SubmissionType.Unaccepted, AnswerResult.NotFound);
        }

        if (instance.FlagContext is null && submission.Challenge.Type.IsStatic())
        {
            updateSub.Status = await context.FlagContexts
                .AsNoTracking()
                .AnyAsync(
                    f => f.ChallengeId == submission.ChallengeId && f.Flag == submission.Answer,
                    token)
                ? AnswerResult.Accepted : AnswerResult.WrongAnswer;
        }
        else
        {
            updateSub.Status = instance.FlagContext?.Flag == submission.Answer
                ? AnswerResult.Accepted : AnswerResult.WrongAnswer;
        }

        bool firstTime = !instance.IsSolved && updateSub.Status == AnswerResult.Accepted;

        if (firstTime && submission.Game.EndTimeUTC > submission.SubmitTimeUTC)
        {
            instance.IsSolved = true;
            updateSub.Challenge.AcceptedCount++;
            ret = updateSub.Challenge.AcceptedCount switch
            {
                1 => SubmissionType.FirstBlood,
                2 => SubmissionType.SecondBlood,
                3 => SubmissionType.ThirdBlood,
                _ => SubmissionType.Normal
            };
        }
        else
        {
            ret = updateSub.Status == AnswerResult.Accepted ?
                SubmissionType.Normal : SubmissionType.Unaccepted;
        }

        await SaveAsync(token);

        return (ret, updateSub.Status);
    }
}
