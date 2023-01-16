using System.Linq;
using System.Net.Mime;
using CTFServer.Extensions;
using CTFServer.Middlewares;
using CTFServer.Models.Internal;
using CTFServer.Models.Request.Account;
using CTFServer.Models.Request.Admin;
using CTFServer.Models.Request.Info;
using CTFServer.Repositories.Interface;
using CTFServer.Services.Interface;
using CTFServer.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CTFServer.Controllers;

/// <summary>
/// Administrative interfaces
/// </summary>
[RequireAdmin]
[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(RequestResponse), StatusCodes.Status403Forbidden)]
public class AdminController : ControllerBase
{
    private readonly UserManager<UserInfo> userManager;
    private readonly ILogRepository logRepository;
    private readonly IFileRepository fileService;
    private readonly IConfigService configService;
    private readonly IGameRepository gameRepository;
    private readonly ITeamRepository teamRepository;
    private readonly IServiceProvider serviceProvider;
    private readonly IParticipationRepository participationRepository;
    private readonly string basepath;

    public AdminController(UserManager<UserInfo> _userManager,
        IFileRepository _FileService,
        ILogRepository _logRepository,
        IConfigService _configService,
        IGameRepository _gameRepository,
        ITeamRepository _teamRepository,
        IServiceProvider _serviceProvider,
        IConfiguration _configuration,
        IParticipationRepository _participationRepository)
    {
        userManager = _userManager;
        fileService = _FileService;
        configService = _configService;
        logRepository = _logRepository;
        teamRepository = _teamRepository;
        gameRepository = _gameRepository;
        serviceProvider = _serviceProvider;
        participationRepository = _participationRepository;
        basepath = _configuration.GetSection("UploadFolder").Value ?? "uploads";
    }

    /// <summary>
    /// Get global settings
    /// </summary>
    /// <remarks>
    /// Gets global settings (Admin permission required)
    /// </remarks>
    /// <response code="200">Global settings</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [HttpGet("Config")]
    [ProducesResponseType(typeof(ConfigEditModel), StatusCodes.Status200OK)]
    public IActionResult GetConfigs()
    {
        // always reload, ensure latest
        configService.ReloadConfig();

        ConfigEditModel config = new()
        {
            AccountPolicy = serviceProvider.GetRequiredService<IOptionsSnapshot<AccountPolicy>>().Value,
            GlobalConfig = serviceProvider.GetRequiredService<IOptionsSnapshot<GlobalConfig>>().Value
        };

        return Ok(config);
    }

    /// <summary>
    /// Change global settings
    /// </summary>
    /// <remarks>
    /// Updates global settings (Admin permission required)
    /// </remarks>
    /// <response code="200">Updated global settings successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [HttpPut("Config")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateConfigs([FromBody] ConfigEditModel model, CancellationToken token)
    {
        foreach (var prop in typeof(ConfigEditModel).GetProperties())
        {
            var value = prop.GetValue(model);
            if (value is not null)
                await configService.SaveConfig(prop.PropertyType, value, token);
        }

        return Ok();
    }

    /// <summary>
    /// Get all users
    /// </summary>
    /// <remarks>
    /// Gets specified number of users (Admin permission required)
    /// </remarks>
    /// <response code="200">User List</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [HttpGet("Users")]
    [ProducesResponseType(typeof(ArrayResponse<UserInfoModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Users([FromQuery] int count = 100, [FromQuery] int skip = 0, CancellationToken token = default)
        => Ok((await (
            from user in userManager.Users.OrderBy(e => e.Id).Skip(skip).Take(count)
            select UserInfoModel.FromUserInfo(user)
           ).ToArrayAsync(token)).ToResponse(await userManager.Users.CountAsync(token)));

    /// <summary>
    /// Search users
    /// </summary>
    /// <remarks>
    /// Searches users by hint (Admin permission required)
    /// </remarks>
    /// <response code="200">User List</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [HttpPost("Users/Search")]
    [ProducesResponseType(typeof(ArrayResponse<UserInfoModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchUsers([FromQuery] string hint, CancellationToken token = default)
        => Ok((await (
            from user in userManager.Users
                .Where(item =>
                    EF.Functions.Like(item.UserName!, $"%{hint}%") ||
                    EF.Functions.Like(item.StdNumber, $"%{hint}%") ||
                    EF.Functions.Like(item.Email!, $"%{hint}%") ||
                    EF.Functions.Like(item.Id, $"%{hint}%") ||
                    EF.Functions.Like(item.RealName, $"%{hint}%")
                )
                .OrderBy(e => e.Id).Take(30)
            select UserInfoModel.FromUserInfo(user)
           ).ToArrayAsync(token)).ToResponse());

    /// <summary>
    /// Get team list
    /// </summary>
    /// <remarks>
    /// Gets specified number of teams (Admin permission required)
    /// </remarks>
    /// <response code="200">User List</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [HttpGet("Teams")]
    [ProducesResponseType(typeof(ArrayResponse<TeamInfoModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Teams([FromQuery] int count = 100, [FromQuery] int skip = 0, CancellationToken token = default)
        => Ok((await teamRepository.GetTeams(count, skip, token))
                .Select(team => TeamInfoModel.FromTeam(team))
                .ToResponse(await teamRepository.CountAsync(token)));

    /// <summary>
    /// Search team
    /// </summary>
    /// <remarks>
    /// Searches for team (Admin permission required)
    /// </remarks>
    /// <response code="200">User List</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [HttpPost("Teams/Search")]
    [ProducesResponseType(typeof(ArrayResponse<TeamInfoModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchTeams([FromQuery] string hint, CancellationToken token = default)
        => Ok((await teamRepository.SearchTeams(hint, token))
                .Select(team => TeamInfoModel.FromTeam(team))
                .ToResponse());

    /// <summary>
    /// Modify user information
    /// </summary>
    /// <remarks>
    /// Modifies user information (Admin permission required)
    /// </remarks>
    /// <response code="200">Modified user information successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="404">User not found</response>
    [HttpPut("Users/{userid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserInfo(string userid, [FromBody] UpdateUserInfoModel model, CancellationToken token)
    {
        var user = await userManager.FindByIdAsync(userid);

        if (user is null)
            return NotFound(new RequestResponse("User not found", 404));

        user.UpdateUserInfo(model);
        await userManager.UpdateAsync(user);

        return Ok();
    }

    /// <summary>
    /// Reset user password
    /// </summary>
    /// <remarks>
    /// Resets user password, (Admin permission required)
    /// </remarks>
    /// <response code="200">Reset succeeded</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="404">User not found</response>
    [HttpDelete("Users/{userid}/Password")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPassword(string userid, CancellationToken token = default)
    {
        var user = await userManager.FindByIdAsync(userid);

        if (user is null)
            return NotFound(new RequestResponse("User not found", 404));

        var pwd = Codec.RandomPassword(16);
        var code = await userManager.GeneratePasswordResetTokenAsync(user);
        await userManager.ResetPasswordAsync(user, code, pwd);

        return Ok(pwd);
    }

    /// <summary>
    /// Delete user
    /// </summary>
    /// <remarks>
    /// Deletes user (Admin permission required)
    /// </remarks>
    /// <response code="200">Delete succeeded</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="404">User not found</response>
    [HttpDelete("Users/{userid}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(string userid, CancellationToken token = default)
    {
        var user = await userManager.GetUserAsync(User);

        if (user!.Id == userid)
            return BadRequest(new RequestResponse("Cannot delete yourself"));

        user = await userManager.FindByIdAsync(userid);

        if (user is null)
            return NotFound(new RequestResponse("User not found", 404));

        if (await teamRepository.CheckIsCaptain(user, token))
            return BadRequest(new RequestResponse("Cannot delete captain"));

        await userManager.DeleteAsync(user);

        return Ok();
    }

    /// <summary>
    /// Delete Team
    /// </summary>
    /// <remarks>
    /// Deletes team, (Admin permission required)
    /// </remarks>
    /// <response code="200">Delete succeeded</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="404">Team not found</response>
    [HttpDelete("Teams/{id}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTeam(int id, CancellationToken token = default)
    {
        var team = await teamRepository.GetTeamById(id);

        if (team is null)
            return NotFound(new RequestResponse("Team not found", 404));

        await teamRepository.DeleteTeam(team, token);

        return Ok();
    }

    /// <summary>
    /// Get user information
    /// </summary>
    /// <remarks>
    /// Gets user information (Admin permission required)
    /// </remarks>
    /// <response code="200">User object</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [HttpGet("Users/{userid}")]
    [ProducesResponseType(typeof(ProfileUserInfoModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UserInfo(string userid)
    {
        var user = await userManager.FindByIdAsync(userid);

        if (user is null)
            return NotFound(new RequestResponse("User not found", 404));

        return Ok(ProfileUserInfoModel.FromUserInfo(user));
    }

    /// <summary>
    /// Get all logs
    /// </summary>
    /// <remarks>
    /// Gets specified number of logs (Admin permission required)
    /// </remarks>
    /// <response code="200">Log list</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [HttpGet("Logs")]
    [ProducesResponseType(typeof(LogMessageModel[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logs([FromQuery] string? level = "All", [FromQuery] int count = 50, [FromQuery] int skip = 0, CancellationToken token = default)
        => Ok(await logRepository.GetLogs(skip, count, level, token));

    /// <summary>
    /// Update team participation status
    /// </summary>
    /// <remarks>
    /// Updates team participation status (Admin permission required)
    /// </remarks>
    /// <response code="200">Update succeeded</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="404">Participant status not found</response>
    [HttpPut("Participation/{id}/{status}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Participation(int id, ParticipationStatus status, CancellationToken token = default)
    {
        var participation = await participationRepository.GetParticipationById(id, token);

        if (participation is null)
            return NotFound(new RequestResponse("Participant status not found", 404));

        await participationRepository.UpdateParticipationStatus(participation, status, token);

        return Ok();
    }

    /// <summary>
    /// Get game writeups
    /// </summary>
    /// <remarks>
    /// Gets game writeups (Admin permission required)
    /// </remarks>
    /// <response code="200">Writeup list</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="404">Game not found</response>
    [HttpGet("Writeups/{id}")]
    [ProducesResponseType(typeof(WriteupInfoModel[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Writeups(int id, CancellationToken token = default)
    {
        var game = await gameRepository.GetGameById(id, token);

        if (game is null)
            return NotFound(new RequestResponse("Game not found", 404));

        return Ok(await participationRepository.GetWriteups(game, token));
    }

    /// <summary>
    /// Download game writeups
    /// </summary>
    /// <remarks>
    /// Downloads game writeups (Admin permission required)
    /// </remarks>
    /// <response code="200">Download succeeded</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="404">Game not found</response>
    [HttpGet("Writeups/{id}/All")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadAllWriteups(int id, CancellationToken token = default)
    {
        var game = await gameRepository.GetGameById(id, token);

        if (game is null)
            return NotFound(new RequestResponse("Game not found", 404));

        var wps = await participationRepository.GetWriteups(game, token);
        var filename = $"Writeups-{game.Title}-{DateTimeOffset.UtcNow:yyyyMMdd-HH.mm.ssZ}";
        var stream = await Codec.ZipFilesAsync(wps.Select(p => p.File), basepath, filename, token);
        stream.Seek(0, SeekOrigin.Begin);

        return File(stream, "application/zip", $"{filename}.zip");
    }

    /// <summary>
    /// Get all files
    /// </summary>
    /// <remarks>
    /// Gets specified number of files (Admin permission required)
    /// </remarks>
    /// <response code="200">File list</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [HttpGet("Files")]
    [ProducesResponseType(typeof(List<LocalFile>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Files([FromQuery] int count = 50, [FromQuery] int skip = 0, CancellationToken token = default)
        => Ok(await fileService.GetFiles(count, skip, token));
}
