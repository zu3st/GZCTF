using System.Threading.Channels;
using CTFServer.Repositories.Interface;
using CTFServer.Utils;

namespace CTFServer.Services;

public static class ChannelService
{
    internal static IServiceCollection AddChannel<T>(this IServiceCollection services)
    {
        var channel = Channel.CreateUnbounded<T>();
        services.AddSingleton(channel);
        services.AddSingleton(channel.Reader);
        services.AddSingleton(channel.Writer);
        return services;
    }
}

public class FlagChecker : IHostedService
{
    private readonly ILogger<FlagChecker> logger;
    private readonly ChannelReader<Submission> channelReader;
    private readonly ChannelWriter<Submission> channelWriter;
    private readonly IServiceScopeFactory serviceScopeFactory;

    private CancellationTokenSource TokenSource { get; set; } = new CancellationTokenSource();

    public FlagChecker(ChannelReader<Submission> _channelReader,
        ChannelWriter<Submission> _channelWriter,
        ILogger<FlagChecker> _logger,
        IServiceScopeFactory _serviceScopeFactory)
    {
        logger = _logger;
        channelReader = _channelReader;
        channelWriter = _channelWriter;
        serviceScopeFactory = _serviceScopeFactory;
    }

    private async Task Checker(int id, CancellationToken token = default)
    {
        logger.SystemLog($"Checker thread #{id} started", TaskStatus.Pending, LogLevel.Debug);

        try
        {
            await foreach (var item in channelReader.ReadAllAsync(token))
            {
                logger.SystemLog($"Checker thread #{id} started processing submission: {item.Answer}", TaskStatus.Pending, LogLevel.Debug);

                await using var scope = serviceScopeFactory.CreateAsyncScope();

                var eventRepository = scope.ServiceProvider.GetRequiredService<IGameEventRepository>();
                var instanceRepository = scope.ServiceProvider.GetRequiredService<IInstanceRepository>();
                var gameNoticeRepository = scope.ServiceProvider.GetRequiredService<IGameNoticeRepository>();
                var gameRepository = scope.ServiceProvider.GetRequiredService<IGameRepository>();
                var submissionRepository = scope.ServiceProvider.GetRequiredService<ISubmissionRepository>();

                try
                {
                    var (type, ans) = await instanceRepository.VerifyAnswer(item, token);

                    if (ans == AnswerResult.NotFound)
                        logger.Log($"[Instance not found] Team [{item.Team.Name}] submitted answer [{item.Answer}] for challenge [{item.Challenge.Title}]", item.User!, TaskStatus.NotFound, LogLevel.Warning);
                    else if (ans == AnswerResult.Accepted)
                    {
                        logger.Log($"[Submission accepted] Team [{item.Team.Name}] submitted answer [{item.Answer}] for challenge [{item.Challenge.Title}]", item.User!, TaskStatus.Success, LogLevel.Information);

                        await eventRepository.AddEvent(GameEvent.FromSubmission(item, type, ans), token);
                    }
                    else
                    {
                        logger.Log($"[Submission failed] Team [{item.Team.Name}] submitted answer [{item.Answer}] for challenge [{item.Challenge.Title}]", item.User!, TaskStatus.Fail, LogLevel.Information);

                        await eventRepository.AddEvent(GameEvent.FromSubmission(item, type, ans), token);

                        var result = await instanceRepository.CheckCheat(item, token);
                        ans = result.AnswerResult;

                        if (ans == AnswerResult.CheatDetected)
                        {
                            logger.Log($"[Cheat check] Team [{item.Team.Name}] suspected of cheating in [{item.Challenge.Title}], related teams [{result.SourceTeam!.Name}]", item.User!, TaskStatus.Success, LogLevel.Information);
                            await eventRepository.AddEvent(new()
                            {
                                Type = EventType.CheatDetected,
                                Content = $"Suspected cheating in challenge [{item.Challenge.Title}], related teams [{item.Team.Name}] and [{result.SourceTeam!.Name}]",
                                TeamId = item.TeamId,
                                UserId = item.UserId,
                                GameId = item.GameId,
                            }, token);
                        }
                    }

                    if (item.Game.EndTimeUTC > DateTimeOffset.UtcNow
                        && type != SubmissionType.Unaccepted
                        && type != SubmissionType.Normal)
                        await gameNoticeRepository.AddNotice(GameNotice.FromSubmission(item, type), token);

                    item.Status = ans;
                    await submissionRepository.SendSubmission(item);

                    gameRepository.FlushScoreboardCache(item.GameId);
                }
                catch (Exception e)
                {
                    logger.SystemLog($"Checker thread #{id} encountered an exception", TaskStatus.Fail, LogLevel.Debug);
                    logger.LogError(e.Message, e);
                }

                token.ThrowIfCancellationRequested();
            }
        }
        catch (OperationCanceledException)
        {
            logger.SystemLog($"Task cancelled, checker thread #{id} will exit", TaskStatus.Exit, LogLevel.Debug);
        }
        finally
        {
            logger.SystemLog($"Checker thread #{id} exited", TaskStatus.Exit, LogLevel.Debug);
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        TokenSource = new CancellationTokenSource();

        for (int i = 0; i < 4; ++i)
            _ = Checker(i, TokenSource.Token);

        await using var scope = serviceScopeFactory.CreateAsyncScope();

        var submissionRepository = scope.ServiceProvider.GetRequiredService<ISubmissionRepository>();
        var flags = await submissionRepository.GetUncheckedFlags(TokenSource.Token);

        foreach (var item in flags)
            await channelWriter.WriteAsync(item, TokenSource.Token);

        if (flags.Length > 0)
            logger.SystemLog($"Restarted checking {flags.Length} flags", TaskStatus.Pending, LogLevel.Debug);

        logger.SystemLog("Flag checker started", TaskStatus.Success, LogLevel.Debug);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        TokenSource.Cancel();

        logger.SystemLog("Flag checker stopped", TaskStatus.Exit, LogLevel.Debug);

        return Task.CompletedTask;
    }
}
