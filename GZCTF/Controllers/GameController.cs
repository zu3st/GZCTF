using System.Net.Mime;
using System.Security.Claims;
using System.Threading.Channels;
using CTFServer.Middlewares;
using CTFServer.Models;
using CTFServer.Models.Request.Admin;
using CTFServer.Models.Request.Edit;
using CTFServer.Models.Request.Game;
using CTFServer.Repositories.Interface;
using CTFServer.Utils;
using k8s.KubeConfigModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CTFServer.Controllers;

/// <summary>
/// Game data interaction interface
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(RequestResponse), StatusCodes.Status403Forbidden)]
public class GameController : ControllerBase
{
    private readonly ILogger<GameController> logger;
    private readonly UserManager<UserInfo> userManager;
    private readonly ChannelWriter<Submission> checkerChannelWriter;
    private readonly IFileRepository fileService;
    private readonly IGameRepository gameRepository;
    private readonly ITeamRepository teamRepository;
    private readonly IContainerRepository containerRepository;
    private readonly IGameNoticeRepository noticeRepository;
    private readonly IGameEventRepository eventRepository;
    private readonly IInstanceRepository instanceRepository;
    private readonly IChallengeRepository challengeRepository;
    private readonly ISubmissionRepository submissionRepository;
    private readonly IParticipationRepository participationRepository;
    private readonly IGameEventRepository gameEventRepository;

    public GameController(
        ILogger<GameController> _logger,
        UserManager<UserInfo> _userManager,
        ChannelWriter<Submission> _channelWriter,
        IFileRepository _fileService,
        IGameRepository _gameRepository,
        ITeamRepository _teamRepository,
        IGameEventRepository _eventRepository,
        IGameNoticeRepository _noticeRepository,
        IInstanceRepository _instanceRepository,
        IChallengeRepository _challengeRepository,
        IContainerRepository _containerRepository,
        IGameEventRepository _gameEventRepository,
        ISubmissionRepository _submissionRepository,
        IParticipationRepository _participationRepository)
    {
        logger = _logger;
        userManager = _userManager;
        checkerChannelWriter = _channelWriter;
        fileService = _fileService;
        gameRepository = _gameRepository;
        teamRepository = _teamRepository;
        eventRepository = _eventRepository;
        noticeRepository = _noticeRepository;
        instanceRepository = _instanceRepository;
        challengeRepository = _challengeRepository;
        containerRepository = _containerRepository;
        gameEventRepository = _gameEventRepository;
        submissionRepository = _submissionRepository;
        participationRepository = _participationRepository;
    }

    /// <summary>
    /// Get the latest game
    /// </summary>
    /// <remarks>
    /// Gets the latest ten games
    /// </remarks>
    /// <param name="token"></param>
    /// <response code="200">List of the latest ten games</response>
    [HttpGet]
    [ProducesResponseType(typeof(BasicGameInfoModel[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> Games(CancellationToken token)
         => Ok(await gameRepository.GetBasicGameInfo(10, 0, token));

    /// <summary>
    /// Get game details
    /// </summary>
    /// <remarks>
    /// Gets the details of a game
    /// </remarks>
    /// <param name="id">Game id</param>
    /// <param name="token"></param>
    /// <response code="200">Game information</response>
    /// <response code="404">Game not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DetailedGameInfoModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Games(int id, CancellationToken token)
    {
        var context = await GetContextInfo(id, token: token);

        if (context.Game is null)
            return NotFound(new RequestResponse("Game not found"));

        var count = await participationRepository.GetParticipationCount(context.Game, token);

        return Ok(DetailedGameInfoModel.FromGame(context.Game, count)
                      .WithParticipation(context.Participation));
    }

    /// <summary>
    /// Join a game
    /// </summary>
    /// <remarks>
    /// Joins in a game, (User permission required)
    /// </remarks>
    /// <param name="id">Game id</param>
    /// <param name="model"></param>
    /// <param name="token"></param>
    /// <response code="200">Joined game successfully</response>
    /// <response code="403">No permission or invalid operation</response>
    /// <response code="404">Game not found</response>
    [RequireUser]
    [HttpPost("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> JoinGame(int id, [FromBody] GameJoinModel model, CancellationToken token)
    {
        var game = await gameRepository.GetGameById(id, token);

        if (game is null)
            return NotFound(new RequestResponse("Game not found", 404));

        if (!game.PracticeMode && game.EndTimeUTC < DateTimeOffset.UtcNow)
            return BadRequest(new RequestResponse("The competition is over"));

        if (!string.IsNullOrEmpty(game.InviteCode) && game.InviteCode != model.InviteCode)
            return BadRequest(new RequestResponse("Bad invite code"));

        if (game.Organizations is { Count: > 0 } && game.Organizations.All(o => o != model.Organization))
            return BadRequest(new RequestResponse("Invalid organization"));

        var user = await userManager.GetUserAsync(User);
        var team = await teamRepository.GetTeamById(model.TeamId, token);

        if (team is null)
            return NotFound(new RequestResponse("Team not found", 404));

        if (team.Members.All(u => u.Id != user!.Id))
            return BadRequest(new RequestResponse("You are not a member of the team"));

        // If already participating (not rejected)
        if (await participationRepository.CheckRepeatParticipation(user!, game, token))
            return BadRequest(new RequestResponse("You have already signed up for another team"));

        // Remove all existing participations
        await participationRepository.RemoveUserParticipations(user!, game, token);

        // Get the registration information by team and game
        var part = await participationRepository.GetParticipation(team, game, token);

        // If the team is not registered
        if (part is null)
        {
            // Create a new team participation object, do not add a triple
            part = new()
            {
                Game = game,
                Team = team,
                Organization = model.Organization,
                Token = gameRepository.GetToken(game, team)
            };

            participationRepository.Add(part);
        }

        if (game.TeamMemberCountLimit > 0 && part.Members.Count >= game.TeamMemberCountLimit)
            return BadRequest(new RequestResponse("The number of participants in the team exceeds the competition limit"));

        // Register the current member
        part.Members.Add(new(user!, game, team));

        part.Organization = model.Organization;

        if (part.Status == ParticipationStatus.Denied)
            part.Status = ParticipationStatus.Pending;

        await participationRepository.SaveAsync(token);

        if (game.AcceptWithoutReview)
            await participationRepository.UpdateParticipationStatus(part, ParticipationStatus.Accepted, token);

        logger.Log($"[{team!.Name}] successfully joined the game [{game.Title}]", user, TaskStatus.Success);

        return Ok();
    }

    /// <summary>
    /// Leave a game
    /// </summary>
    /// <remarks>
    /// Leaves a game, (User permission required)
    /// </remarks>
    /// <param name="id">Game id </param>
    /// <param name="token"></param>
    /// <response code="200">Left game successfully</response>
    /// <response code="403">No permission or invalid operation</response>
    /// <response code="404">Game not found</response>
    [RequireUser]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> LeaveGame(int id, CancellationToken token)
    {
        var game = await gameRepository.GetGameById(id, token);

        if (game is null)
            return NotFound(new RequestResponse("Game not found", 404));

        var user = await userManager.GetUserAsync(User);

        var part = await participationRepository.GetParticipation(user!, game, token);

        if (part is null || part.Members.All(u => u.UserId != user!.Id))
            return BadRequest(new RequestResponse("Unable to leave a game that has not been joined"));

        if (part.Status != ParticipationStatus.Pending && part.Status != ParticipationStatus.Denied)
            return BadRequest(new RequestResponse("Unable to leave a game after approval"));


        // FIXME: After approval, new users can be added, but they cannot exit?

        part.Members.RemoveWhere(u => u.UserId == user!.Id);

        if (part.Members.Count == 0)
            await participationRepository.RemoveParticipation(part, token);
        else
            await participationRepository.SaveAsync(token);

        return Ok();
    }

    /// <summary>
    /// Get Scoreboard
    /// </summary>
    /// <remarks>
    /// Gets the scoreboard of the game
    /// </remarks>
    /// <param name="id">Game id</param>
    /// <param name="token"></param>
    /// <response code="200">Scoreboard information</response>
    /// <response code="400">Game has not started yet</response>
    /// <response code="404">Game not found</response>
    [HttpGet("{id}/Scoreboard")]
    [ProducesResponseType(typeof(ScoreboardModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Scoreboard([FromRoute] int id, CancellationToken token)
    {
        var game = await gameRepository.GetGameById(id, token);

        if (game is null)
            return NotFound(new RequestResponse("Game not found"));

        if (DateTimeOffset.UtcNow < game.StartTimeUTC)
            return BadRequest(new RequestResponse("Game has not started yet"));

        return Ok(await gameRepository.GetScoreboard(game, token));
    }

    /// <summary>
    /// Get game notices
    /// </summary>
    /// <remarks>
    /// Gets specified number of game notices
    /// </remarks>
    /// <param name="id">Game id</param>
    /// <param name="count"></param>
    /// <param name="skip"></param>
    /// <param name="token"></param>
    /// <response code="200">List of notices</response>
    /// <response code="400">Game has not started yet</response>
    /// <response code="404">Game not found</response>
    [HttpGet("{id}/Notices")]
    [ProducesResponseType(typeof(GameNotice[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Notices([FromRoute] int id, [FromQuery] int count = 100, [FromQuery] int skip = 0, CancellationToken token = default)
    {
        var game = await gameRepository.GetGameById(id, token);

        if (game is null)
            return NotFound(new RequestResponse("Game not found"));

        if (DateTimeOffset.UtcNow < game.StartTimeUTC)
            return BadRequest(new RequestResponse("Game has not started yet"));

        return Ok(await noticeRepository.GetNotices(game.Id, count, skip, token));
    }

    /// <summary>
    /// Get game events
    /// </summary>
    /// <remarks>
    /// Gets specified number of game events (Monitor permission required)
    /// </remarks>
    /// <param name="id">Game id</param>
    /// <param name="count"></param>
    /// <param name="hideContainer">隐藏容器</param>
    /// <param name="skip"></param>
    /// <param name="token"></param>
    /// <response code="200">List of game events</response>
    /// <response code="400">Game has not started yet</response>
    /// <response code="404">Game not found</response>
    [RequireMonitor]
    [HttpGet("{id}/Events")]
    [ProducesResponseType(typeof(GameEvent[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Events([FromRoute] int id, [FromQuery] bool hideContainer = false, [FromQuery] int count = 100, [FromQuery] int skip = 0, CancellationToken token = default)
    {
        var game = await gameRepository.GetGameById(id, token);

        if (game is null)
            return NotFound(new RequestResponse("Game not found"));

        if (DateTimeOffset.UtcNow < game.StartTimeUTC)
            return BadRequest(new RequestResponse("Game has not started yet"));

        return Ok(await eventRepository.GetEvents(game.Id, hideContainer, count, skip, token));
    }

    /// <summary>
    /// Get game submissions
    /// </summary>
    /// <remarks>
    /// Gets specified number of game submissions (Monitor permission required)
    /// </remarks>
    /// <param name="id">Game id</param>
    /// <param name="type">Submission type</param>
    /// <param name="count"></param>
    /// <param name="skip"></param>
    /// <param name="token"></param>
    /// <response code="200">List of submissions</response>
    /// <response code="400">Game has not started yet</response>
    /// <response code="404">Game not found</response>
    [RequireMonitor]
    [HttpGet("{id}/Submissions")]
    [ProducesResponseType(typeof(Submission[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Submissions([FromRoute] int id, [FromQuery] AnswerResult? type = null, [FromQuery] int count = 100, [FromQuery] int skip = 0, CancellationToken token = default)
    {
        var game = await gameRepository.GetGameById(id, token);

        if (game is null)
            return NotFound(new RequestResponse("Game not found"));

        if (DateTimeOffset.UtcNow < game.StartTimeUTC)
            return BadRequest(new RequestResponse("Game has not started yet"));

        return Ok(await submissionRepository.GetSubmissions(game, type, count, skip, token));
    }

    /// <summary>
    /// Get game details of a team
    /// </summary>
    /// <remarks>
    /// Gets game specific details of a team (requires User permission and team must be in the game)
    /// </remarks>
    /// <param name="id">Game id</param>
    /// <param name="token"></param>
    /// <response code="200">Game details</response>
    /// <response code="400">Bad request</response>
    /// <response code="404">Game not found</response>
    [RequireUser]
    [HttpGet("{id}/Details")]
    [ProducesResponseType(typeof(GameDetailModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChallengesWithTeamInfo([FromRoute] int id, CancellationToken token)
    {
        var context = await GetContextInfo(id, token: token);

        if (context.Result is not null)
            return context.Result;

        var scoreboard = await gameRepository.GetScoreboard(context.Game!, token);

        var boarditem = scoreboard.Items.FirstOrDefault(i => i.Id == context.Participation!.TeamId);

        // make sure team info is not null
        boarditem ??= new ScoreboardItem()
        {
            Avatar = context.Participation!.Team.AvatarUrl,
            SolvedCount = 0,
            Rank = 0,
            Name = context.Participation!.Team.Name,
            Id = context.Participation!.TeamId
        };

        return Ok(new GameDetailModel()
        {
            ScoreboardItem = boarditem,
            TeamToken = context.Participation!.Token,
            Challenges = scoreboard.Challenges,
            WriteupDeadline = context.Game!.WriteupDeadline
        });
    }

    /// <summary>
    /// Get all game participations
    /// </summary>
    /// <remarks>
    /// Get all participations of a game (Admin permission required)
    /// </remarks>
    /// <param name="id">Game id</param>
    /// <param name="token"></param>
    /// <response code="200">Game participations</response>
    /// <response code="400">Bad request</response>
    /// <response code="404">Game not found</response>
    [RequireAdmin]
    [HttpGet("{id}/Participations")]
    [ProducesResponseType(typeof(ParticipationInfoModel[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Participations([FromRoute] int id, CancellationToken token = default)
    {
        var context = await GetContextInfo(id, token: token);

        if (context.Game is null)
            return NotFound(new RequestResponse("Game not found"));

        return Ok((await participationRepository.GetParticipations(context.Game!, token))
                    .Select(ParticipationInfoModel.FromParticipation));
    }

    /// <summary>
    /// Download game scoreboard
    /// </summary>
    /// <remarks>
    /// Downloads game scoreboard (Monitor permission required)
    /// </remarks>
    /// <param name="id">Game id</param>
    /// <param name="token"></param>
    /// <response code="200">Downloaded scoreboard successfully</response>
    /// <response code="400">Game has not started yet</response>
    /// <response code="404">Game not found</response>
    [RequireMonitor]
    [HttpGet("{id}/ScoreboardSheet")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ScoreboardSheet([FromRoute] int id, CancellationToken token = default)
    {
        var game = await gameRepository.GetGameById(id, token);

        if (game is null)
            return NotFound(new RequestResponse("Game not found"));

        if (DateTimeOffset.UtcNow < game.StartTimeUTC)
            return BadRequest(new RequestResponse("Game has not started yet"));

        var scoreboard = await gameRepository.GetScoreboard(game, token);

        var stream = ExcelHelper.GetScoreboardExcel(scoreboard, game);
        stream.Seek(0, SeekOrigin.Begin);

        return File(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{game.Title}_Scoreboard_{DateTimeOffset.Now:yyyyMMddHHmmss}.xlsx");
    }

    /// <summary>
    /// Download spreadsheet of all submissions
    /// </summary>
    /// <remarks>
    /// Downloads a XLSX spreadsheet of all submissions of a game (Monitor permission required)
    /// </remarks>
    /// <param name="id">Game id</param>
    /// <param name="token"></param>
    /// <response code="200">Downloaded spreadsheet successfully</response>
    /// <response code="400">Game has not started yet</response>
    /// <response code="404">Game not found</response>
    [RequireMonitor]
    [HttpGet("{id}/SubmissionSheet")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmissionSheet([FromRoute] int id, CancellationToken token = default)
    {
        var game = await gameRepository.GetGameById(id, token);

        if (game is null)
            return NotFound(new RequestResponse("Game not found"));

        if (DateTimeOffset.UtcNow < game.StartTimeUTC)
            return BadRequest(new RequestResponse("Game has not started yet"));

        var submissions = await submissionRepository.GetSubmissions(game, count: 0, token: token);

        var stream = ExcelHelper.GetSubmissionExcel(submissions, game);
        stream.Seek(0, SeekOrigin.Begin);

        return File(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{game.Title}_Submissions_{DateTimeOffset.Now:yyyyMMddHHmmss}.xlsx");
    }

    /// <summary>
    /// Get game challenge info
    /// </summary>
    /// <remarks>
    /// Gets game challenge info, (User permission required and must be in the game)
    /// </remarks>
    /// <param name="id">Game id</param>
    /// <param name="challengeId">Challenge id</param>
    /// <param name="token"></param>
    /// <response code="200">Game challenge info</response>
    /// <response code="400">Bad request</response>
    /// <response code="404">Game not found</response>
    [RequireUser]
    [HttpGet("{id}/Challenges/{challengeId}")]
    [ProducesResponseType(typeof(ChallengeDetailModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChallenge([FromRoute] int id, [FromRoute] int challengeId, CancellationToken token)
    {
        if (id <= 0 || challengeId <= 0)
            return NotFound(new RequestResponse("Game not found", 404));

        var context = await GetContextInfo(id, token: token);

        if (context.Result is not null)
            return context.Result;

        var instance = await instanceRepository.GetInstance(context.Participation!, challengeId, token);

        if (instance is null)
            return NotFound(new RequestResponse("Challenge not found or dynamic attachment allocation failed", 404));

        return Ok(ChallengeDetailModel.FromInstance(instance));
    }

    /// <summary>
    /// Submit flag to checker queue
    /// </summary>
    /// <remarks>
    /// Submits flag for later checking (User permission required and must be in the game)
    /// </remarks>
    /// <param name="id">Game id</param>
    /// <param name="challengeId">Challenge id</param>
    /// <param name="model">Flag text</param>
    /// <param name="token"></param>
    /// <response code="200">Submitted flag successfully</response>
    /// <response code="400">Bad request</response>
    /// <response code="404">Game not found</response>
    [RequireUser]
    [HttpPost("{id}/Challenges/{challengeId}")]
    [EnableRateLimiting(nameof(RateLimiter.LimitPolicy.Submit))]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Submit([FromRoute] int id, [FromRoute] int challengeId, [FromBody] FlagSubmitModel model, CancellationToken token)
    {
        var context = await GetContextInfo(id, challengeId, false, token: token);

        if (context.Result is not null)
            return context.Result;

        Submission submission = new()
        {
            Answer = model.Flag.Trim(),
            Game = context.Game!,
            User = context.User!,
            Challenge = context.Challenge!,
            Team = context.Participation!.Team,
            Participation = context.Participation!,
            Status = AnswerResult.FlagSubmitted,
            SubmitTimeUTC = DateTimeOffset.UtcNow,
        };

        submission = await submissionRepository.AddSubmission(submission, token);

        // Send to flag checker service
        await checkerChannelWriter.WriteAsync(submission, token);

        return Ok(submission.Id);
    }

    /// <summary>
    /// Query flag status
    /// </summary>
    /// <remarks>
    /// Queries flag status (User permission required)
    /// </remarks>
    /// <param name="id">Game id</param>
    /// <param name="challengeId">Challenge id</param>
    /// <param name="submitId">Submission id</param>
    /// <param name="token"></param>
    /// <response code="200">Flag status</response>
    /// <response code="404">Submission not found</response>
    [RequireUser]
    [HttpGet("{id}/Challenges/{challengeId}/Status/{submitId}")]
    [ProducesResponseType(typeof(AnswerResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Status([FromRoute] int id, [FromRoute] int challengeId, [FromRoute] int submitId, CancellationToken token)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var submission = await submissionRepository.GetSubmission(id, challengeId, userId!, submitId, token);

        if (submission is null)
            return NotFound(new RequestResponse("Submission not found", 404));

        return Ok(submission.Status switch
        {
            AnswerResult.CheatDetected => AnswerResult.WrongAnswer,
            var x => x,
        });
    }

    /// <summary>
    /// Get Writeup
    /// </summary>
    /// <remarks>
    /// Gets Writeup (User permission required)
    /// </remarks>
    /// <param name="id"></param>
    /// <param name="token"></param>
    /// <response code="200">Writeup info</response>
    /// <response code="400">Bad request</response>
    /// <response code="404">Game not found</response>
    [RequireUser]
    [HttpGet("{id}/Writeup")]
    [ProducesResponseType(typeof(BasicWriteupInfoModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetWriteup([FromRoute] int id, CancellationToken token)
    {
        var context = await GetContextInfo(id, denyAfterEnded: false, token: token);

        if (context.Result is not null)
            return context.Result;

        return Ok(BasicWriteupInfoModel.FromParticipation(context.Participation!));
    }

    /// <summary>
    /// Submit Writeup
    /// </summary>
    /// <remarks>
    /// Submits Writeup (User permission required)
    /// </remarks>
    /// <param name="id"></param>
    /// <param name="file">File</param>
    /// <param name="token"></param>
    /// <response code="200">Submitted writeup successfully</response>
    /// <response code="400">Bad request</response>
    /// <response code="404">Game not found</response>
    [RequireUser]
    [HttpPost("{id}/Writeup")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitWriteup([FromRoute] int id, IFormFile file, CancellationToken token)
    {
        if (file.Length == 0)
            return BadRequest(new RequestResponse("File is invalid"));

        if (file.Length > 20 * 1024 * 1024)
            return BadRequest(new RequestResponse("File is too large"));

        if (file.ContentType != "application/pdf" || Path.GetExtension(file.FileName) != ".pdf")
            return BadRequest(new RequestResponse("Please upload a pdf file"));

        var context = await GetContextInfo(id, denyAfterEnded: false, token: token);

        if (context.Result is not null)
            return context.Result;

        var game = context.Game!;
        var part = context.Participation!;
        var team = part.Team;

        if (DateTimeOffset.UtcNow > game.WriteupDeadline)
            return BadRequest(new RequestResponse("Writeup submission deadline has passed"));

        var wp = context.Participation!.Writeup;

        if (wp is not null)
            await fileService.DeleteFile(wp, token);

        wp = await fileService.CreateOrUpdateFile(file, $"Writeup-{game.Id}-{team.Id}-{DateTimeOffset.Now:yyyyMMdd-HH.mm.ssZ}.pdf", token);

        if (wp is null)
            return BadRequest(new RequestResponse("Failed to save file"));

        part.Writeup = wp;

        await participationRepository.SaveAsync(token);

        logger.Log($"{team.Name} team successfully submitted Writeup for {game.Title}", context.User!, TaskStatus.Success);

        return Ok();
    }

    /// <summary>
    /// Create Container instance
    /// </summary>
    /// <remarks>
    /// Creates a container instance for a challenge (User permission required)
    /// </remarks>
    /// <param name="id">Game id</param>
    /// <param name="challengeId">Challenge id</param>
    /// <param name="token"></param>
    /// <response code="200">Created container successfully</response>
    /// <response code="404">Challenge not found</response>
    /// <response code="400">Challenge type does not support container</response>
    /// <response code="429">Too frequent container operations</response>
    [RequireUser]
    [HttpPost("{id}/Container/{challengeId}")]
    [EnableRateLimiting(nameof(RateLimiter.LimitPolicy.Container))]
    [ProducesResponseType(typeof(ContainerInfoModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CreateContainer([FromRoute] int id, [FromRoute] int challengeId, CancellationToken token)
    {
        var context = await GetContextInfo(id, token: token);

        if (context.Result is not null)
            return context.Result;

        var instance = await instanceRepository.GetInstance(context.Participation!, challengeId, token);

        if (instance is null)
            return NotFound(new RequestResponse("Challenge not found", 404));

        if (!instance.Challenge.Type.IsContainer())
            return BadRequest(new RequestResponse("Challenge type does not support container", 400));

        if (DateTimeOffset.UtcNow - instance.LastContainerOperation < TimeSpan.FromSeconds(10))
            return new JsonResult(new RequestResponse("Too frequent container operations", 429))
            {
                StatusCode = 429
            };

        if (instance.Container is not null)
        {
            if (instance.Container.Status == ContainerStatus.Running)
                return BadRequest(new RequestResponse("Container for this challenge already exists"));

            await containerRepository.RemoveContainer(instance.Container, token);
        }

        return await instanceRepository.CreateContainer(instance, context.Participation!.Team, context.User!, context.Game!.ContainerCountLimit, token) switch
        {
            null or (TaskStatus.Fail, null) => BadRequest(new RequestResponse("Failed to create container")),
            (TaskStatus.Denied, null) => BadRequest(new RequestResponse($"Team container count cannot exceed {context.Game.ContainerCountLimit}")),
            (TaskStatus.Success, var x) => Ok(ContainerInfoModel.FromContainer(x!)),
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Prolong challenge container instance
    /// </summary>
    /// <remarks>
    /// Prolongs challenge container instance lifetime, (User permission required and max container lifetime is 2 hours)
    /// </remarks>
    /// <param name="id">Game id</param>
    /// <param name="challengeId">Challenge id</param>
    /// <param name="token"></param>
    /// <response code="200">Prolonged container successfully</response>
    /// <response code="404">Instance not found</response>
    /// <response code="400">Failed to prolong container</response>
    [RequireUser]
    [HttpPost("{id}/Container/{challengeId}/Prolong")]
    [EnableRateLimiting(nameof(RateLimiter.LimitPolicy.Container))]
    [ProducesResponseType(typeof(ContainerInfoModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProlongContainer([FromRoute] int id, [FromRoute] int challengeId, CancellationToken token)
    {
        var context = await GetContextInfo(id, token: token);

        if (context.Result is not null)
            return context.Result;

        var instance = await instanceRepository.GetInstance(context.Participation!, challengeId, token);

        if (instance is null)
            return NotFound(new RequestResponse("Instance not found", 404));

        if (!instance.Challenge.Type.IsContainer())
            return BadRequest(new RequestResponse("Challenge type does not support container"));

        if (instance.Container is null)
            return BadRequest(new RequestResponse("Challenge does not have container"));

        if (instance.Container.ExpectStopAt - DateTimeOffset.UtcNow < TimeSpan.FromMinutes(10))
            return BadRequest(new RequestResponse("Container time is not extendable yet"));

        await instanceRepository.ProlongContainer(instance.Container, TimeSpan.FromHours(2), token);

        return Ok(ContainerInfoModel.FromContainer(instance.Container));
    }

    /// <summary>
    /// Delete Container instance
    /// </summary>
    /// <remarks>
    /// Deletes a challenge container instance (User permission required)
    /// </remarks>
    /// <param name="id">Game id</param>
    /// <param name="challengeId">Challenge id</param>
    /// <param name="token"></param>
    /// <response code="200">Deleted container instance successfully</response>
    /// <response code="404">Instance not found</response>
    /// <response code="400">Challenge type does not support container</response>
    /// <response code="429">Too frequent container operations</response>
    [RequireUser]
    [HttpDelete("{id}/Container/{challengeId}")]
    [EnableRateLimiting(nameof(RateLimiter.LimitPolicy.Container))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteContainer([FromRoute] int id, [FromRoute] int challengeId, CancellationToken token)
    {
        var context = await GetContextInfo(id, token: token);

        if (context.Result is not null)
            return context.Result;

        var instance = await instanceRepository.GetInstance(context.Participation!, challengeId, token);

        if (instance is null)
            return NotFound(new RequestResponse("Container instance not found", 404));

        if (!instance.Challenge.Type.IsContainer())
            return BadRequest(new RequestResponse("Challenge type does not support container"));

        if (instance.Container is null)
            return BadRequest(new RequestResponse("Challenge does not have container"));

        if (DateTimeOffset.UtcNow - instance.LastContainerOperation < TimeSpan.FromSeconds(10))
            return new JsonResult(new RequestResponse("Too frequent container operations", 429))
            {
                StatusCode = 429
            };

        var destroyId = instance.Container.Id;

        if (!await instanceRepository.DestroyContainer(instance.Container, token))
            return BadRequest(new RequestResponse("Failed to delete container"));

        instance.LastContainerOperation = DateTimeOffset.UtcNow;

        await gameEventRepository.AddEvent(new()
        {
            Type = EventType.ContainerDestroy,
            GameId = context.Game!.Id,
            TeamId = context.Participation!.TeamId,
            UserId = context.User!.Id,
            Content = $"Destroyed container instance of {instance.Challenge.Title}#{instance.Challenge.Id}"
        }, token);

        logger.Log($"{context.Participation!.Team.Name} destroyed container instance of {instance.Challenge.Title} [{destroyId}]", context.User, TaskStatus.Success);
        return Ok();
    }

    private class ContextInfo
    {
        public Game? Game = default!;
        public UserInfo? User = default!;
        public Challenge? Challenge = default!;
        public Participation? Participation = default!;
        public IActionResult? Result = null;

        public ContextInfo WithResult(IActionResult res)
        {
            Result = res;
            return this;
        }
    };

    private async Task<ContextInfo> GetContextInfo(int id, int challengeId = 0, bool withFlag = false, bool denyAfterEnded = true, CancellationToken token = default)
    {
        ContextInfo res = new()
        {
            User = await userManager.GetUserAsync(User),
            Game = await gameRepository.GetGameById(id, token)
        };

        if (res.Game is null)
            return res.WithResult(NotFound(new RequestResponse("Game not found", 404)));

        var part = await participationRepository.GetParticipation(res.User!, res.Game, token);

        if (part is null)
            return res.WithResult(BadRequest(new RequestResponse("You are not participating in this game")));

        res.Participation = part;

        if (part.Status != ParticipationStatus.Accepted)
            return res.WithResult(BadRequest(new RequestResponse("Your participation request has not been accepted or you are banned")));

        if (DateTimeOffset.UtcNow < res.Game.StartTimeUTC)
            return res.WithResult(BadRequest(new RequestResponse("Game has not started yet")));

        if (denyAfterEnded && !res.Game.PracticeMode && res.Game.EndTimeUTC < DateTimeOffset.UtcNow)
            return res.WithResult(BadRequest(new RequestResponse("Game has ended")));

        if (challengeId > 0)
        {
            var challenge = await challengeRepository.GetChallenge(id, challengeId, withFlag, token);

            if (challenge is null)
                return res.WithResult(NotFound(new RequestResponse("Challenge not found", 404)));

            res.Challenge = challenge;
        }

        return res;
    }
}
