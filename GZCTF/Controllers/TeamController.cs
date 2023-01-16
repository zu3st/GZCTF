using System.Net.Mime;
using System.Text.RegularExpressions;
using CTFServer.Middlewares;
using CTFServer.Models.Request.Info;
using CTFServer.Repositories.Interface;
using CTFServer.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace CTFServer.Controllers;

/// <summary>
/// Team data interaction interface
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class TeamController : ControllerBase
{
    private readonly UserManager<UserInfo> userManager;
    private readonly IFileRepository FileService;
    private readonly ITeamRepository teamRepository;
    private readonly IParticipationRepository participationRepository;
    private readonly ILogger<TeamController> logger;

    public TeamController(UserManager<UserInfo> _userManager,
        IFileRepository _FileService,
        ILogger<TeamController> _logger,
        ITeamRepository _teamRepository,
        IParticipationRepository _participationRepository)
    {
        logger = _logger;
        userManager = _userManager;
        FileService = _FileService;
        teamRepository = _teamRepository;
        participationRepository = _participationRepository;
    }

    /// <summary>
    /// Get team information
    /// </summary>
    /// <remarks>
    /// Get basic information of a team by id
    /// </remarks>
    /// <param name="id">Team id</param>
    /// <param name="token"></param>
    /// <response code="200">Team information</response>
    /// <response code="400">Team does not exist</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TeamInfoModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetBasicInfo(int id, CancellationToken token)
    {
        var team = await teamRepository.GetTeamById(id, token);

        if (team is null)
            return NotFound(new RequestResponse("Team does not exist", 404));

        return Ok(TeamInfoModel.FromTeam(team));
    }

    /// <summary>
    /// Get your own team information
    /// </summary>
    /// <remarks>
    /// Get basic information of a team by user
    /// </remarks>
    /// <param name="token"></param>
    /// <response code="200">Team information</response>
    /// <response code="400">Team does not exist</response>
    [HttpGet]
    [RequireUser]
    [ProducesResponseType(typeof(TeamInfoModel[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTeamsInfo(CancellationToken token)
    {
        var user = await userManager.GetUserAsync(User);

        return Ok((await teamRepository.GetUserTeams(user!, token)).Select(t => TeamInfoModel.FromTeam(t)));
    }

    /// <summary>
    /// Create a team
    /// </summary>
    /// <remarks>
    /// Create a team, each user can only create one team
    /// </remarks>
    /// <param name="model"></param>
    /// <param name="token"></param>
    /// <response code="200">Team information</response>
    /// <response code="400">Team does not exist</response>
    [HttpPost]
    [RequireUser]
    [EnableRateLimiting(nameof(RateLimiter.LimitPolicy.Concurrency))]
    [ProducesResponseType(typeof(TeamInfoModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateTeam([FromBody] TeamUpdateModel model, CancellationToken token)
    {
        var user = await userManager.GetUserAsync(User);

        var teams = await teamRepository.GetUserTeams(user!, token);

        if (teams.Length > 1 && teams.Any(t => t.CaptainId == user!.Id))
            return BadRequest(new RequestResponse("You are not allowed to create multiple teams"));

        if (string.IsNullOrEmpty(model.Name))
            return BadRequest(new RequestResponse("Team name cannot be empty"));

        var team = await teamRepository.CreateTeam(model, user!, token);

        if (team is null)
            return BadRequest(new RequestResponse("Team creation failed"));

        await userManager.UpdateAsync(user!);

        logger.Log($"Created team {team.Name}", user, TaskStatus.Success);

        return Ok(TeamInfoModel.FromTeam(team));
    }

    /// <summary>
    /// Update team information
    /// </summary>
    /// <remarks>
    /// Updates team information (must be captain)
    /// </remarks>
    /// <param name="id">Team id</param>
    /// <param name="model"></param>
    /// <param name="token"></param>
    /// <response code="200">Updated team information successfully</response>
    /// <response code="400">Team not found</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [HttpPut("{id}")]
    [RequireUser]
    [ProducesResponseType(typeof(TeamInfoModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateTeam([FromRoute] int id, [FromBody] TeamUpdateModel model, CancellationToken token)
    {
        var user = await userManager.GetUserAsync(User);
        var team = await teamRepository.GetTeamById(id, token);

        if (team is null)
            return BadRequest(new RequestResponse("Team not found"));

        if (team.CaptainId != user!.Id)
            return new JsonResult(new RequestResponse("Forbidden", 403)) { StatusCode = 403 };

        team.UpdateInfo(model);

        await teamRepository.SaveAsync(token);

        return Ok(TeamInfoModel.FromTeam(team));
    }

    /// <summary>
    /// Transfer team ownership
    /// </summary>
    /// <remarks>
    /// Transfers team ownership to another user (must be captain)
    /// </remarks>
    /// <param name="id">Team id</param>
    /// <param name="model"></param>
    /// <param name="token"></param>
    /// <response code="200">Transfer team ownership successfully</response>
    /// <response code="400">Team not found</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [HttpPut("{id}/Transfer")]
    [RequireUser]
    [ProducesResponseType(typeof(TeamInfoModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Transfer([FromRoute] int id, [FromBody] TeamTransferModel model, CancellationToken token)
    {
        var user = await userManager.GetUserAsync(User);
        var team = await teamRepository.GetTeamById(id, token);

        if (team is null)
            return BadRequest(new RequestResponse("Team not found"));

        if (team.CaptainId != user!.Id)
            return new JsonResult(new RequestResponse("Forbidden", 403)) { StatusCode = 403 };

        if (team.Locked && await teamRepository.AnyActiveGame(team, token))
            return BadRequest(new RequestResponse("Team is locked"));

        var newCaptain = await userManager.Users.SingleOrDefaultAsync(u => u.Id == model.NewCaptainId);

        if (newCaptain is null)
            return BadRequest(new RequestResponse("User to transfer to does not exist"));

        var newCaptainTeams = await teamRepository.GetUserTeams(newCaptain, token);

        if (newCaptainTeams.Count(t => t.CaptainId == newCaptain.Id) >= 3)
            return BadRequest(new RequestResponse("User to transfer to already manages too many teams"));

        await teamRepository.Transfer(team, newCaptain, token);

        return Ok(TeamInfoModel.FromTeam(team));
    }

    /// <summary>
    /// Get invite info
    /// </summary>
    /// <remarks>
    /// Gets team invite info, (must be captain)
    /// </remarks>
    /// <param name="id">Team id</param>
    /// <param name="token"></param>
    /// <response code="200">Team Token</response>
    /// <response code="400">Team not found</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [HttpGet("{id}/Invite")]
    [RequireUser]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> InviteCode([FromRoute] int id, CancellationToken token)
    {
        var user = await userManager.GetUserAsync(User);
        var team = await teamRepository.GetTeamById(id, token);

        if (team is null)
            return BadRequest(new RequestResponse("Team not found"));

        if (team.CaptainId != user!.Id)
            return new JsonResult(new RequestResponse("Forbidden", 403)) { StatusCode = 403 };

        return Ok(team.InviteCode);
    }

    /// <summary>
    /// Update invite token
    /// </summary>
    /// <remarks>
    /// Updates team invite token, (must be captain)
    /// </remarks>
    /// <param name="id">Team id</param>
    /// <param name="token"></param>
    /// <response code="200">Updated team token successfully</response>
    /// <response code="400">Team not found</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [HttpPut("{id}/Invite")]
    [RequireUser]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateInviteToken([FromRoute] int id, CancellationToken token)
    {
        var user = await userManager.GetUserAsync(User);
        var team = await teamRepository.GetTeamById(id, token);

        if (team is null)
            return BadRequest(new RequestResponse("Team not found"));

        if (team.CaptainId != user!.Id)
            return new JsonResult(new RequestResponse("Forbidden", 403)) { StatusCode = 403 };

        team.UpdateInviteToken();

        await teamRepository.SaveAsync(token);

        return Ok(team.InviteCode);
    }

    /// <summary>
    /// Kick user from team
    /// </summary>
    /// <remarks>
    /// Removes user from team (must be captain)
    /// </remarks>
    /// <param name="id">Team id</param>
    /// <param name="userid">User id to kick</param>
    /// <param name="token"></param>
    /// <response code="200">Removed user from team successfully</response>
    /// <response code="400">Team not found</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [HttpPost("{id}/Kick/{userid}")]
    [RequireUser]
    [ProducesResponseType(typeof(TeamInfoModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> KickUser([FromRoute] int id, [FromRoute] string userid, CancellationToken token)
    {
        var user = await userManager.GetUserAsync(User);
        var team = await teamRepository.GetTeamById(id, token);

        if (team is null)
            return BadRequest(new RequestResponse("Team not found"));

        if (team.CaptainId != user!.Id)
            return new JsonResult(new RequestResponse("Forbidden", 403)) { StatusCode = 403 };

        var trans = await teamRepository.BeginTransactionAsync(token);

        try
        {
            if (team.Locked && await teamRepository.AnyActiveGame(team, token))
                return BadRequest(new RequestResponse("Team is locked"));

            var kickUser = team.Members.SingleOrDefault(m => m.Id == userid);
            if (kickUser is null)
                return BadRequest(new RequestResponse("User is not in team"));

            team.Members.Remove(kickUser);
            await participationRepository.RemoveUserParticipations(user, team, token);

            await teamRepository.SaveAsync(token);
            await trans.CommitAsync(token);

            logger.Log($"Kicked {kickUser.UserName} from team {team.Name}", user, TaskStatus.Success);
            return Ok(TeamInfoModel.FromTeam(team));
        }
        catch
        {
            await trans.RollbackAsync(token);
            throw;
        }
    }

    /// <summary>
    /// Accept team invite
    /// </summary>
    /// <remarks>
    /// Accept team invite (requires User privileges and must not be in a team)
    /// </remarks>
    /// <param name="code">Team invite code</param>
    /// <param name="cancelToken"></param>
    /// <response code="200">Accepted team invite successfully</response>
    /// <response code="400">Team does not exist</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [HttpPost("Accept")]
    [RequireUser]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Accept([FromBody] string code, CancellationToken cancelToken)
    {
        if (!Regex.IsMatch(code, @":\d+:[0-9a-f]{32}"))
            return BadRequest(new RequestResponse("Invalid invite code"));

        var inviteCode = code[^32..];
        var preCode = code[..^33];

        var lastColon = preCode.LastIndexOf(':');

        if (!int.TryParse(preCode[(lastColon + 1)..], out var teamId))
            return BadRequest(new RequestResponse($"Team Id conversion error: {preCode[(lastColon + 1)..]}"));

        var teamName = preCode[..lastColon];
        var trans = await teamRepository.BeginTransactionAsync(cancelToken);

        try
        {
            var team = await teamRepository.GetTeamById(teamId, cancelToken);

            if (team is null)
                return BadRequest(new RequestResponse($"{teamName} team not found"));

            if (team.InviteCode != code)
                return BadRequest(new RequestResponse("Invalid invite code"));

            if (team.Locked && await teamRepository.AnyActiveGame(team, cancelToken))
                return BadRequest(new RequestResponse($"{teamName} team is locked"));

            var user = await userManager.GetUserAsync(User);

            if (team.Members.Any(m => m.Id == user!.Id))
                return BadRequest(new RequestResponse("You are already in this team"));

            team.Members.Add(user!);

            await teamRepository.SaveAsync(cancelToken);
            await trans.CommitAsync(cancelToken);

            logger.Log($"Joined team {team.Name}", user, TaskStatus.Success);
            return Ok();
        }
        catch
        {
            await trans.RollbackAsync(cancelToken);
            throw;
        }
    }

    /// <summary>
    /// Leave team
    /// </summary>
    /// <remarks>
    /// Leaves team (requires User privilege and must be in a team)
    /// </remarks>
    /// <param name="id">Team id</param>
    /// <param name="token"></param>
    /// <response code="200">Left team successfully</response>
    /// <response code="400">Team does not exist</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [HttpPost("{id}/Leave")]
    [RequireUser]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Leave([FromRoute] int id, CancellationToken token)
    {
        var trans = await teamRepository.BeginTransactionAsync(token);

        try
        {
            var team = await teamRepository.GetTeamById(id, token);

            if (team is null)
                return BadRequest(new RequestResponse("Team not found"));

            var user = await userManager.GetUserAsync(User);

            if (team.Members.All(m => m.Id != user!.Id))
                return BadRequest(new RequestResponse("You are not in this team"));

            if (team.Locked && await teamRepository.AnyActiveGame(team, token))
                return BadRequest(new RequestResponse("Teams are locked"));

            team.Members.Remove(user!);

            await teamRepository.SaveAsync(token);
            await trans.CommitAsync(token);

            logger.Log($"Left team {team.Name}", user, TaskStatus.Success);
            return Ok();
        }
        catch
        {
            await trans.RollbackAsync(token);
            throw;
        }
    }

    /// <summary>
    /// Update team avatar
    /// </summary>
    /// <remarks>
    /// Updates team avatar (requires User privilege and must be captain)
    /// </remarks>
    /// <response code="200">Updated team avatar URL</response>
    /// <response code="400">Bad request</response>
    /// <response code="401">Unauthorized</response>
    [HttpPut("{id}/Avatar")]
    [RequireUser]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Avatar([FromRoute] int id, IFormFile file, CancellationToken token)
    {
        var user = await userManager.GetUserAsync(User);
        var team = await teamRepository.GetTeamById(id, token);

        if (team is null)
            return BadRequest(new RequestResponse("Team not found"));

        if (team.CaptainId != user!.Id)
            return new JsonResult(new RequestResponse("无权访问", 403)) { StatusCode = 403 };

        if (file.Length == 0)
            return BadRequest(new RequestResponse("Invalid file"));

        if (file.Length > 3 * 1024 * 1024)
            return BadRequest(new RequestResponse("File too large"));

        if (team.AvatarHash is not null)
            _ = await FileService.DeleteFileByHash(team.AvatarHash, token);

        var avatar = await FileService.CreateOrUpdateFile(file, "avatar", token);

        if (avatar is null)
            return BadRequest(new RequestResponse("Invalid file"));

        team.AvatarHash = avatar.Hash;
        await teamRepository.SaveAsync(token);

        logger.Log($"Team {team.Name} changed avatar to [{avatar.Hash[..8]}]", user, TaskStatus.Success);

        return Ok(avatar.Url());
    }

    /// <summary>
    /// Delete team
    /// </summary>
    /// <remarks>
    /// Deletes a team (User privilege required and must be captain)
    /// </remarks>
    /// <param name="id">Team id</param>
    /// <param name="token"></param>
    /// <response code="200">Deleted team successfully</response>
    /// <response code="400">Team not found</response>
    [HttpDelete("{id}")]
    [RequireUser]
    [ProducesResponseType(typeof(TeamInfoModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteTeam(int id, CancellationToken token)
    {
        var user = await userManager.GetUserAsync(User);
        var team = await teamRepository.GetTeamById(id, token);

        if (team is null)
            return BadRequest(new RequestResponse("Team not found"));

        if (team.CaptainId != user!.Id)
            return new JsonResult(new RequestResponse("Forbidden", 403)) { StatusCode = 403 };

        if (team.Locked && await teamRepository.AnyActiveGame(team, token))
            return BadRequest(new RequestResponse("Teams are locked"));

        await teamRepository.DeleteTeam(team!, token);

        logger.Log($"Deleted team {team!.Name}", user, TaskStatus.Success);

        return Ok();
    }
}
