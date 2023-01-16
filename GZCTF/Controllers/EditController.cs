using System.Net.Mime;
using CTFServer.Extensions;
using CTFServer.Middlewares;
using CTFServer.Models.Request.Edit;
using CTFServer.Models.Request.Game;
using CTFServer.Models.Request.Info;
using CTFServer.Repositories.Interface;
using CTFServer.Services.Interface;
using CTFServer.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CTFServer.Controllers;

/// <summary>
/// Data modification interaction interface
/// </summary>
[RequireAdmin]
[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(RequestResponse), StatusCodes.Status403Forbidden)]
public class EditController : Controller
{
    private readonly ILogger<EditController> logger;
    private readonly UserManager<UserInfo> userManager;
    private readonly IPostRepository postRepository;
    private readonly IGameNoticeRepository gameNoticeRepository;
    private readonly IGameRepository gameRepository;
    private readonly IChallengeRepository challengeRepository;
    private readonly IFileRepository fileService;
    private readonly IContainerService containerService;
    private readonly IContainerRepository containerRepository;

    public EditController(UserManager<UserInfo> _userManager,
        ILogger<EditController> _logger,
        IPostRepository _postRepository,
        IContainerRepository _containerRepository,
        IChallengeRepository _challengeRepository,
        IGameNoticeRepository _gameNoticeRepository,
        IGameRepository _gameRepository,
        IContainerService _containerService,
        IFileRepository _fileService)
    {
        logger = _logger;
        fileService = _fileService;
        userManager = _userManager;
        gameRepository = _gameRepository;
        postRepository = _postRepository;
        containerService = _containerService;
        challengeRepository = _challengeRepository;
        containerRepository = _containerRepository;
        gameNoticeRepository = _gameNoticeRepository;
    }

    /// <summary>
    /// Add a post
    /// </summary>
    /// <remarks>
    /// Adds a post (Admin permission required)
    /// </remarks>
    /// <param name="model"></param>
    /// <param name="token"></param>
    /// <response code="200">Added post successfully</response>
    [HttpPost("Posts")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddPost([FromBody] PostEditModel model, CancellationToken token)
    {
        var user = await userManager.GetUserAsync(User);
        var res = await postRepository.CreatePost(new Post().Update(model, user!), token);
        return Ok(res.Id);
    }

    /// <summary>
    /// Update post
    /// </summary>
    /// <remarks>
    /// Updates post (Admin permission required)
    /// </remarks>
    /// <param name="id">Post id</param>
    /// <param name="token"></param>
    /// <param name="model"></param>
    /// <response code="200">Edited post successfully</response>
    /// <response code="404">Post not found</response>
    [HttpPut("Posts/{id}")]
    [ProducesResponseType(typeof(PostDetailModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePost(string id, [FromBody] PostEditModel model, CancellationToken token)
    {
        var post = await postRepository.GetPostById(id, token);

        if (post is null)
            return NotFound(new RequestResponse("Post not found", 404));

        var user = await userManager.GetUserAsync(User);

        await postRepository.UpdatePost(post.Update(model, user!), token);

        return Ok(PostDetailModel.FromPost(post));
    }

    /// <summary>
    /// Delete post
    /// </summary>
    /// <remarks>
    /// Deletes post (Admin permission required)
    /// </remarks>
    /// <param name="id">Post id</param>
    /// <param name="token"></param>
    /// <response code="200">Deleted post successfully</response>
    /// <response code="404">Post not found</response>
    [HttpDelete("Posts/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePost(string id, CancellationToken token)
    {
        var post = await postRepository.GetPostById(id, token);

        if (post is null)
            return NotFound(new RequestResponse("Post not found", 404));

        await postRepository.RemovePost(post, token);

        return Ok();
    }

    /// <summary>
    /// Add game
    /// </summary>
    /// <remarks>
    /// Adds game (Admin permission required)
    /// </remarks>
    /// <param name="model"></param>
    /// <param name="token"></param>
    /// <response code="200">Added game successfully</response>
    [HttpPost("Games")]
    [ProducesResponseType(typeof(GameInfoModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddGame([FromBody] GameInfoModel model, CancellationToken token)
    {
        var game = await gameRepository.CreateGame(new Game().Update(model), token);

        if (game is null)
            return BadRequest(new RequestResponse("Failed to create game", 400));

        gameRepository.FlushGameInfoCache();

        return Ok(GameInfoModel.FromGame(game));
    }

    /// <summary>
    /// Get games
    /// </summary>
    /// <remarks>
    /// Gets specified number of games (Admin permission required)
    /// </remarks>
    /// <param name="count"></param>
    /// <param name="skip"></param>
    /// <param name="token"></param>
    /// <response code="200">List of games</response>
    [HttpGet("Games")]
    [ProducesResponseType(typeof(ArrayResponse<GameInfoModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGames([FromQuery] int count, [FromQuery] int skip, CancellationToken token)
        => Ok((await gameRepository.GetGames(count, skip, token))
            .Select(GameInfoModel.FromGame)
            .ToResponse(await gameRepository.CountAsync(token)));

    /// <summary>
    /// Get game
    /// </summary>
    /// <remarks>
    /// Gets game (Admin permission required)
    /// </remarks>
    /// <param name="id"></param>
    /// <param name="token"></param>
    /// <response code="200">Game info</response>
    [HttpGet("Games/{id}")]
    [ProducesResponseType(typeof(GameInfoModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGame([FromRoute] int id, CancellationToken token)
    {
        var game = await gameRepository.GetGameById(id, token);

        if (game is null)
            return NotFound(new RequestResponse("Game not found", 404));

        return Ok(GameInfoModel.FromGame(game));
    }

    /// <summary>
    /// Update game
    /// </summary>
    /// <remarks>
    /// Updates game (Admin permission required)
    /// </remarks>
    /// <param name="id"></param>
    /// <param name="model"></param>
    /// <param name="token"></param>
    /// <response code="200">Updated game successfully</response>
    [HttpPut("Games/{id}")]
    [ProducesResponseType(typeof(GameInfoModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateGame([FromRoute] int id, [FromBody] GameInfoModel model, CancellationToken token)
    {
        var game = await gameRepository.GetGameById(id, token);

        if (game is null)
            return NotFound(new RequestResponse("Game not found", 404));

        game.Update(model);
        await gameRepository.SaveAsync(token);
        gameRepository.FlushGameInfoCache();
        gameRepository.FlushScoreboardCache(game.Id);

        return Ok(GameInfoModel.FromGame(game));
    }

    /// <summary>
    /// Delete game
    /// </summary>
    /// <remarks>
    /// Deletes game (Admin permission required)
    /// </remarks>
    /// <param name="id"></param>
    /// <param name="token"></param>
    /// <response code="200">Deleted game successfully</response>
    [HttpDelete("Games/{id}")]
    [ProducesResponseType(typeof(GameInfoModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteGame([FromRoute] int id, CancellationToken token)
    {
        var game = await gameRepository.GetGameById(id, token);

        if (game is null)
            return NotFound(new RequestResponse("Game not found", 404));

        await gameRepository.DeleteGame(game, token);

        return Ok();
    }

    /// <summary>
    /// Update game poster
    /// </summary>
    /// <remarks>
    /// Updates game poster (Admin permission required)
    /// </remarks>
    /// <response code="200">Game poster url</response>
    /// <response code="400">Bad request</response>
    /// <response code="401">Unauthorized</response>
    [HttpPut("Games/{id}/Poster")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateGamePoster([FromRoute] int id, IFormFile file, CancellationToken token)
    {
        if (file.Length == 0)
            return BadRequest(new RequestResponse("File is invalid"));

        if (file.Length > 3 * 1024 * 1024)
            return BadRequest(new RequestResponse("File is too large"));

        var game = await gameRepository.GetGameById(id, token);

        if (game is null)
            return NotFound(new RequestResponse("Game not found", 404));

        var poster = await fileService.CreateOrUpdateFile(file, "poster", token);

        if (poster is null)
            return BadRequest(new RequestResponse("File creation failed"));

        game.PosterHash = poster.Hash;
        await gameRepository.SaveAsync(token);
        gameRepository.FlushGameInfoCache();

        return Ok(poster.Url());
    }

    /// <summary>
    /// Add game notice
    /// </summary>
    /// <remarks>
    /// Adds game notice (Admin permission required)
    /// </remarks>
    /// <param name="id">Game id</param>
    /// <param name="model">Notice content</param>
    /// <param name="token"></param>
    /// <response code="200">Added game notice successfully</response>
    [HttpPost("Games/{id}/Notices")]
    [ProducesResponseType(typeof(GameNotice), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddGameNotice([FromRoute] int id, [FromBody] GameNoticeModel model, CancellationToken token)
    {
        var game = await gameRepository.GetGameById(id, token);

        if (game is null)
            return NotFound(new RequestResponse("Game not found", 404));

        var res = await gameNoticeRepository.AddNotice(new()
        {
            Content = model.Content,
            GameId = game.Id,
            Type = NoticeType.Normal,
            PublishTimeUTC = DateTimeOffset.UtcNow
        }, token);

        return Ok(res);
    }

    /// <summary>
    /// Get game notices
    /// </summary>
    /// <remarks>
    /// Gets game notices (Admin permission required)
    /// </remarks>
    /// <param name="id">Game id</param>
    /// <param name="token"></param>
    /// <response code="200">Game notices</response>
    [HttpGet("Games/{id}/Notices")]
    [ProducesResponseType(typeof(GameNotice[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGameNotices([FromRoute] int id, CancellationToken token = default)
    {
        var game = await gameRepository.GetGameById(id, token);

        if (game is null)
            return NotFound(new RequestResponse("Game not found", 404));

        return Ok(await gameNoticeRepository.GetNormalNotices(id, token));
    }

    /// <summary>
    /// Update game notice
    /// </summary>
    /// <remarks>
    /// Updates game notice (Admin permission required)
    /// </remarks>
    /// <param name="id">Game id</param>
    /// <param name="noticeId">Notice id</param>
    /// <param name="model">Notice content</param>
    /// <param name="token"></param>
    /// <response code="200">Updated game notice successfully</response>
    [HttpPut("Games/{id}/Notices/{noticeId}")]
    [ProducesResponseType(typeof(GameNotice), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateGameNotice([FromRoute] int id, [FromRoute] int noticeId, [FromBody] GameNoticeModel model, CancellationToken token = default)
    {
        var notice = await gameNoticeRepository.GetNoticeById(id, noticeId, token);

        if (notice is null)
            return NotFound(new RequestResponse("Notice not found", 404));

        if (notice.Type != NoticeType.Normal)
            return BadRequest(new RequestResponse("Cannot update system notice"));

        notice.Content = model.Content;
        return Ok(await gameNoticeRepository.UpdateNotice(notice, token));
    }

    /// <summary>
    /// Delete game notice
    /// </summary>
    /// <remarks>
    /// Deletes game notice (Admin permission required)
    /// </remarks>
    /// <param name="id">Game id</param>
    /// <param name="noticeId">Notice id</param>
    /// <param name="token"></param>
    /// <response code="200">Deleted game notice successfully</response>
    /// <response code="404">Notice not found</response>
    [HttpDelete("Games/{id}/Notices/{noticeId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteGameNotice([FromRoute] int id, [FromRoute] int noticeId, CancellationToken token)
    {
        var notice = await gameNoticeRepository.GetNoticeById(id, noticeId, token);

        if (notice is null)
            return NotFound(new RequestResponse("Notice not found", 404));

        if (notice.Type != NoticeType.Normal)
            return BadRequest(new RequestResponse("Cannot delete system notice"));

        await gameNoticeRepository.RemoveNotice(notice, token);

        return Ok();
    }

    /// <summary>
    /// Add game challenge
    /// </summary>
    /// <remarks>
    /// Adds game challenge (Admin permission required)
    /// </remarks>
    /// <param name="id">Game id</param>
    /// <param name="model"></param>
    /// <param name="token"></param>
    /// <response code="200">Added game challenge successfully</response>
    [HttpPost("Games/{id}/Challenges")]
    [ProducesResponseType(typeof(ChallengeEditDetailModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddGameChallenge([FromRoute] int id, [FromBody] ChallengeInfoModel model, CancellationToken token)
    {
        var game = await gameRepository.GetGameById(id, token);

        if (game is null)
            return NotFound(new RequestResponse("Game not found", 404));

        var res = await challengeRepository.CreateChallenge(game, new Challenge()
        {
            Title = model.Title,
            Type = model.Type,
            Tag = model.Tag
        }, token);

        return Ok(ChallengeEditDetailModel.FromChallenge(res));
    }

    /// <summary>
    /// Get all challenges of a game
    /// </summary>
    /// <remarks>
    /// Accesses all challenges of a game (Admin permission required)
    /// </remarks>
    /// <param name="id">Game id</param>
    /// <param name="token"></param>
    /// <response code="200">List of challenges</response>
    [HttpGet("Games/{id}/Challenges")]
    [ProducesResponseType(typeof(ChallengeInfoModel[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGameChallenges([FromRoute] int id, CancellationToken token)
        => Ok((await challengeRepository.GetChallenges(id, token)).Select(ChallengeInfoModel.FromChallenge));

    /// <summary>
    /// Get game challenge
    /// </summary>
    /// <remarks>
    /// Accesses game challenge (Admin permission required)
    /// </remarks>
    /// <param name="id">比赛Id</param>
    /// <param name="cId">题目Id</param>
    /// <param name="token"></param>
    /// <response code="200">Challenge info</response>
    [HttpGet("Games/{id}/Challenges/{cId}")]
    [ProducesResponseType(typeof(ChallengeEditDetailModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGameChallenge([FromRoute] int id, [FromRoute] int cId, CancellationToken token)
    {
        var game = await gameRepository.GetGameById(id, token);

        if (game is null)
            return NotFound(new RequestResponse("比赛未找到", 404));

        var res = await challengeRepository.GetChallenge(id, cId, true, token);

        if (res is null)
            return NotFound(new RequestResponse("题目未找到", 404));

        return Ok(ChallengeEditDetailModel.FromChallenge(res));
    }

    /// <summary>
    /// Update challenge info
    /// </summary>
    /// <remarks>
    /// Flags are not editable, use flag related API to edit flags (Admin permission required)
    /// </remarks>
    /// <param name="id">Game id</param>
    /// <param name="cId">Challenge id</param>
    /// <param name="model">Challenge information</param>
    /// <param name="token"></param>
    /// <response code="200">Updated game challenge successfully</response>
    [HttpPut("Games/{id}/Challenges/{cId}")]
    [ProducesResponseType(typeof(ChallengeEditDetailModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateGameChallenge([FromRoute] int id, [FromRoute] int cId, [FromBody] ChallengeUpdateModel model, CancellationToken token)
    {
        var game = await gameRepository.GetGameById(id, token);

        if (game is null)
            return NotFound(new RequestResponse("Game not found", 404));

        var res = await challengeRepository.GetChallenge(id, cId, true, token);

        if (res is null)
            return NotFound(new RequestResponse("Challenge not found", 404));

        // NOTE: IsEnabled can only be updated outside of the edit page
        if (model.IsEnabled == true && !res.Flags.Any() && res.Type != ChallengeType.DynamicContainer)
            return BadRequest(new RequestResponse("Challenge has no flag, cannot be enabled"));

        if (model.FileName is not null && string.IsNullOrWhiteSpace(model.FileName))
            return BadRequest(new RequestResponse("Dynamic attachment name cannot be empty"));

        bool hintUpdated = model.Hints is not null &&
                model.Hints.Count > 0 &&
                model.Hints.GetSetHashCode() != res.Hints?.GetSetHashCode();

        if (!string.IsNullOrWhiteSpace(model.FlagTemplate)
            && res.Type == ChallengeType.DynamicContainer
            && !model.FlagTemplate.Contains("[TEAM_HASH]")
            && Codec.Leet.LeetEntropy(model.FlagTemplate) < 32.0)
            return BadRequest(new RequestResponse("Flag complexity is too low, please consider adding team hash or increasing length"));

        res.Update(model);

        if (model.IsEnabled == true)
        {
            // Will also update IsEnabled
            await challengeRepository.EnsureInstances(res, game, token);

            if (game.IsActive)
            {
                await gameNoticeRepository.AddNotice(new()
                {
                    Game = game,
                    Type = NoticeType.NewChallenge,
                    Content = $"New challenge ⌈{res.Title}⌋ added",
                }, token);
            }
        }
        else
            await challengeRepository.SaveAsync(token);

        if (game.IsActive && res.IsEnabled && hintUpdated)
        {
            await gameNoticeRepository.AddNotice(new()
            {
                Game = game,
                Type = NoticeType.NewHint,
                Content = $"⌈{res.Title}⌋ updated hints",
            }, token);
        }

        // Always flush scoreboard
        gameRepository.FlushScoreboardCache(game.Id);

        return Ok(ChallengeEditDetailModel.FromChallenge(res));
    }

    /// <summary>
    /// Test game challenge container
    /// </summary>
    /// <remarks>
    /// Tests game challenge container (Admin permission required)
    /// </remarks>
    /// <param name="id">Game id</param>
    /// <param name="cId">Challenge id</param>
    /// <param name="token"></param>
    /// <response code="200">Started test container successfully</response>
    [HttpPost("Games/{id}/Challenges/{cId}/Container")]
    [ProducesResponseType(typeof(ContainerInfoModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateTestContainer([FromRoute] int id, [FromRoute] int cId, CancellationToken token)
    {
        var game = await gameRepository.GetGameById(id, token);

        if (game is null)
            return NotFound(new RequestResponse("Game not found", 404));

        var challenge = await challengeRepository.GetChallenge(id, cId, true, token);

        if (challenge is null)
            return NotFound(new RequestResponse("Challenge not found", 404));

        if (!challenge.Type.IsContainer())
            return BadRequest(new RequestResponse("Challenge type cannot create container"));

        if (challenge.ContainerImage is null || challenge.ContainerExposePort is null)
            return BadRequest(new RequestResponse("Container configuration error"));

        var user = await userManager.GetUserAsync(User);

        var container = await containerService.CreateContainerAsync(new()
        {
            TeamId = "admin",
            UserId = user!.Id,
            Flag = challenge.Type.IsDynamic() ? challenge.GenerateTestFlag() : null,
            Image = challenge.ContainerImage,
            CPUCount = challenge.CPUCount ?? 1,
            MemoryLimit = challenge.MemoryLimit ?? 64,
            StorageLimit = challenge.StorageLimit ?? 256,
            ExposedPort = challenge.ContainerExposePort ?? throw new ArgumentException("Container expose port cannot be null"),
        }, token);

        if (container is null)
            return BadRequest(new RequestResponse("Cannot create container"));

        challenge.TestContainer = container;
        await challengeRepository.SaveAsync(token);

        logger.Log($"Successfully created test container {container.ContainerId}", user, TaskStatus.Success);

        return Ok(ContainerInfoModel.FromContainer(container));
    }

    /// <summary>
    /// Destroy test game challenge container
    /// </summary>
    /// <remarks>
    /// Destroys test game challenge container (Admin permission required)
    /// </remarks>
    /// <param name="id">Game id</param>
    /// <param name="cId">Challenge id</param>
    /// <param name="token"></param>
    /// <response code="200">Destroyed test container successfully</response>
    [HttpDelete("Games/{id}/Challenges/{cId}/Container")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DestroyTestContainer([FromRoute] int id, [FromRoute] int cId, CancellationToken token)
    {
        var game = await gameRepository.GetGameById(id, token);

        if (game is null)
            return NotFound(new RequestResponse("Game not found", 404));

        var challenge = await challengeRepository.GetChallenge(id, cId, true, token);

        if (challenge is null)
            return NotFound(new RequestResponse("Challenge not found", 404));

        if (challenge.TestContainer is null)
            return Ok();

        await containerService.DestroyContainerAsync(challenge.TestContainer, token);
        await containerRepository.RemoveContainer(challenge.TestContainer, token);

        return Ok();
    }

    /// <summary>
    /// Remove game challenge
    /// </summary>
    /// <remarks>
    /// Removes game challenge (Admin permission required)
    /// </remarks>
    /// <param name="id">Game id</param>
    /// <param name="cId">Challenge id</param>
    /// <param name="token"></param>
    /// <response code="200">Removed game challenge successfully</response>
    [HttpDelete("Games/{id}/Challenges/{cId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveGameChallenge([FromRoute] int id, [FromRoute] int cId, CancellationToken token)
    {
        var game = await gameRepository.GetGameById(id, token);

        if (game is null)
            return NotFound(new RequestResponse("Game not found", 404));

        var res = await challengeRepository.GetChallenge(id, cId, true, token);

        if (res is null)
            return NotFound(new RequestResponse("Challenge not found", 404));

        await challengeRepository.RemoveChallenge(res, token);

        // Always flush scoreboard
        gameRepository.FlushScoreboardCache(game.Id);

        return Ok();
    }

    /// <summary>
    /// Update game challenge attachment
    /// </summary>
    /// <remarks>
    /// Updates game challenge attachment, only for non-dynamic attachment challenge (Admin permission required)
    /// </remarks>
    /// <param name="id">Game id</param>
    /// <param name="cId">Challenge id</param>
    /// <param name="model"></param>
    /// <param name="token"></param>
    /// <response code="200">Updated challenge attachment successfully</response>
    [HttpPost("Games/{id}/Challenges/{cId}/Attachment")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAttachment([FromRoute] int id, [FromRoute] int cId, [FromBody] AttachmentCreateModel model, CancellationToken token)
    {
        var game = await gameRepository.GetGameById(id, token);

        if (game is null)
            return NotFound(new RequestResponse("Game not found", 404));

        var challenge = await challengeRepository.GetChallenge(id, cId, true, token);

        if (challenge is null)
            return NotFound(new RequestResponse("Challenge not ofund", 404));

        if (challenge.Type == ChallengeType.DynamicAttachment)
            return BadRequest(new RequestResponse("Dynamic attachment challenge should use assets API to upload attachment"));

        await challengeRepository.UpdateAttachment(challenge, model, token);

        return Ok();
    }

    /// <summary>
    /// Add game challenge flag
    /// </summary>
    /// <remarks>
    /// Adds game challenge flag (Admin permission required)
    /// </remarks>
    /// <param name="id">Game id</param>
    /// <param name="cId">Challenge id</param>
    /// <param name="models"></param>
    /// <param name="token"></param>
    /// <response code="200">Added flag successfully</response>
    [HttpPost("Games/{id}/Challenges/{cId}/Flags")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddFlags([FromRoute] int id, [FromRoute] int cId, [FromBody] FlagCreateModel[] models, CancellationToken token)
    {
        var game = await gameRepository.GetGameById(id, token);

        if (game is null)
            return NotFound(new RequestResponse("Game not found", 404));

        var challenge = await challengeRepository.GetChallenge(id, cId, true, token);

        if (challenge is null)
            return NotFound(new RequestResponse("Challenge not found", 404));

        await challengeRepository.AddFlags(challenge, models, token);

        return Ok();
    }

    /// <summary>
    /// Remove game challenge flag
    /// </summary>
    /// <remarks>
    /// Removes game challenge flag (Admin permission required)
    /// </remarks>
    /// <param name="id">Game id</param>
    /// <param name="cId">Challenge id</param>
    /// <param name="fId">Flag id</param>
    /// <param name="token"></param>
    /// <response code="200">Removed flag successfully</response>
    [HttpDelete("Games/{id}/Challenges/{cId}/Flags/{fId}")]
    [ProducesResponseType(typeof(TaskStatus), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFlag([FromRoute] int id, [FromRoute] int cId, [FromRoute] int fId, CancellationToken token)
    {
        var game = await gameRepository.GetGameById(id, token);

        if (game is null)
            return NotFound(new RequestResponse("Game not found", 404));

        var challenge = await challengeRepository.GetChallenge(id, cId, true, token);

        if (challenge is null)
            return NotFound(new RequestResponse("Challenge not found", 404));

        return Ok(await challengeRepository.RemoveFlag(challenge, fId, token));
    }
}
