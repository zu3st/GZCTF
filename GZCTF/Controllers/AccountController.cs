using System.Net.Mime;
using CTFServer.Extensions;
using CTFServer.Middlewares;
using CTFServer.Models.Internal;
using CTFServer.Models.Request.Account;
using CTFServer.Repositories.Interface;
using CTFServer.Services.Interface;
using CTFServer.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace CTFServer.Controllers;

/// <summary>
/// User account-related interfaces
/// </summary>
[ApiController]
[Route("api/[controller]/[action]")]
[Produces(MediaTypeNames.Application.Json)]
public class AccountController : ControllerBase
{
    private readonly ILogger<AccountController> logger;
    private readonly IMailSender mailSender;
    private readonly UserManager<UserInfo> userManager;
    private readonly SignInManager<UserInfo> signInManager;
    private readonly IFileRepository fileService;
    private readonly IRecaptchaExtension recaptcha;
    private readonly IHostEnvironment environment;
    private readonly IOptionsSnapshot<AccountPolicy> accountPolicy;

    public AccountController(
        IMailSender _mailSender,
        IFileRepository _FileService,
        IHostEnvironment _environment,
        IRecaptchaExtension _recaptcha,
        IOptionsSnapshot<AccountPolicy> _accountPolicy,
        UserManager<UserInfo> _userManager,
        SignInManager<UserInfo> _signInManager,
        ILogger<AccountController> _logger)
    {
        recaptcha = _recaptcha;
        mailSender = _mailSender;
        environment = _environment;
        userManager = _userManager;
        signInManager = _signInManager;
        fileService = _FileService;
        accountPolicy = _accountPolicy;
        logger = _logger;
    }

    /// <summary>
    /// User registration interface
    /// </summary>
    /// <remarks>
    /// Registers new user，Dev environment does not verify GToken，Mail-URL：/verify
    /// </remarks>
    /// <param name="model"></param>
    /// <response code="200">Registration succeeded</response>
    /// <response code="400">Verification failed or user already exists</response>
    [HttpPost]
    [EnableRateLimiting(nameof(RateLimiter.LimitPolicy.Register))]
    [ProducesResponseType(typeof(RequestResponse<RegisterStatus>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (!accountPolicy.Value.AllowRegister)
            return BadRequest(new RequestResponse("Registration is disabled"));

        if (accountPolicy.Value.UseGoogleRecaptcha && (
                model.GToken is null || HttpContext.Connection.RemoteIpAddress is null ||
                !await recaptcha.VerifyAsync(model.GToken, HttpContext.Connection.RemoteIpAddress.ToString())
            ))
            return BadRequest(new RequestResponse("Google reCAPTCHA checksum failure"));

        var mailDomain = model.Email!.Split('@')[1];
        if (!string.IsNullOrWhiteSpace(accountPolicy.Value.EmailDomainList) &&
            accountPolicy.Value.EmailDomainList.Split(',').All(d => d != mailDomain))
            return BadRequest(new RequestResponse($"Valid Email TLDs：{accountPolicy.Value.EmailDomainList}"));

        var user = new UserInfo
        {
            UserName = model.UserName,
            Email = model.Email,
            Role = Role.User
        };

        user.UpdateByHttpContext(HttpContext);

        var result = await userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            var current = await userManager.FindByEmailAsync(model.Email);

            if (current is null)
                return BadRequest(new RequestResponse(result.Errors.FirstOrDefault()?.Description ?? "Unknown error"));

            if (await userManager.IsEmailConfirmedAsync(current))
                return BadRequest(new RequestResponse("Account already exists"));

            user = current;
        }

        if (accountPolicy.Value.ActiveOnRegister)
        {
            user.EmailConfirmed = true;
            await userManager.UpdateAsync(user);
            await signInManager.SignInAsync(user, true);

            logger.Log("User successfully registered", user, TaskStatus.Success);
            return Ok(new RequestResponse<RegisterStatus>("Successfully registered", RegisterStatus.LoggedIn, 200));
        }

        if (!accountPolicy.Value.EmailConfirmationRequired)
        {
            logger.Log("User successfully registered, pending administrator confirmaton", user, TaskStatus.Success);
            return Ok(new RequestResponse<RegisterStatus>("Successfully registered, pending administrator confirmation",
                    RegisterStatus.AdminConfirmationRequired, 200));
        }

        logger.Log("Sending user verification email", user, TaskStatus.Pending);

        var token = Codec.Base64.Encode(await userManager.GenerateEmailConfirmationTokenAsync(user));
        if (environment.IsDevelopment())
        {
            logger.Log($"http://{HttpContext.Request.Host}/account/verify?token={token}&email={Codec.Base64.Encode(model.Email)}", user, TaskStatus.Pending, LogLevel.Debug);
        }
        else
        {
            if (!mailSender.SendConfirmEmailUrl(user.UserName, user.Email,
                $"https://{HttpContext.Request.Host}/account/verify?token={token}&email={Codec.Base64.Encode(model.Email)}"))
                return BadRequest(new RequestResponse("Mail cannot be sent, please contact the administrator"));
        }

        return Ok(new RequestResponse<RegisterStatus>("Successful registration, pending email verification",
                    RegisterStatus.EmailConfirmationRequired, 200));
    }

    /// <summary>
    /// User password recovery request interface
    /// </summary>
    /// <remarks>
    /// Requests password recovery, sending recovery email to user, Mail-URL：/reset
    /// </remarks>
    /// <param name="model"></param>
    /// <response code="200">User password reset email sent</response>
    /// <response code="400">Verification failed</response>
    /// <response code="404">User does not exist</response>
    [HttpPost]
    [EnableRateLimiting(nameof(RateLimiter.LimitPolicy.Register))]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Recovery([FromBody] RecoveryModel model)
    {
        if (accountPolicy.Value.UseGoogleRecaptcha && (
                model.GToken is null || HttpContext.Connection.RemoteIpAddress is null ||
                !await recaptcha.VerifyAsync(model.GToken, HttpContext.Connection.RemoteIpAddress.ToString())
            ))
            return BadRequest(new RequestResponse("Google reCAPTCHA verification failure"));

        var user = await userManager.FindByEmailAsync(model.Email!);
        if (user is null)
            return NotFound(new RequestResponse("User does not exist", 404));

        if (!user.EmailConfirmed)
            return NotFound(new RequestResponse("Account not active, please re-register", 404));

        if (!accountPolicy.Value.EmailConfirmationRequired)
            return BadRequest(new RequestResponse("Please contact the administrator to reset your password"));

        logger.Log("Sending user password reset email", HttpContext, TaskStatus.Pending);

        var token = Codec.Base64.Encode(await userManager.GeneratePasswordResetTokenAsync(user));

        if (environment.IsDevelopment())
        {
            logger.Log($"http://{HttpContext.Request.Host}/account/reset?token={token}&email={Codec.Base64.Encode(model.Email)}", user, TaskStatus.Pending, LogLevel.Debug);
        }
        else
        {
            if (!mailSender.SendResetPasswordUrl(user.UserName, user.Email,
                $"https://{HttpContext.Request.Host}/account/reset?token={token}&email={Codec.Base64.Encode(model.Email)}"))
                return BadRequest(new RequestResponse("Mail cannot be sent, please contact the administrator"));
        }

        return Ok(new RequestResponse("Email sent successfully", 200));
    }

    /// <summary>
    /// User password reset interface
    /// </summary>
    /// <remarks>
    /// Resets user password, requiring a valid reset token
    /// </remarks>
    /// <param name="model"></param>
    /// <response code="200">User password reset successfully</response>
    /// <response code="400">Verification failed</response>
    [HttpPost]
    [EnableRateLimiting(nameof(RateLimiter.LimitPolicy.Register))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PasswordReset([FromBody] PasswordResetModel model)
    {
        var user = await userManager.FindByEmailAsync(Codec.Base64.Decode(model.Email));
        if (user is null)
            return BadRequest(new RequestResponse("Invalid email address"));

        user.UpdateByHttpContext(HttpContext);

        var result = await userManager.ResetPasswordAsync(user, Codec.Base64.Decode(model.RToken), model.Password);

        if (!result.Succeeded)
            return BadRequest(new RequestResponse(result.Errors.FirstOrDefault()?.Description ?? "Unknown error"));

        logger.Log("Successfully reset user's password", user, TaskStatus.Success);

        return Ok();
    }

    /// <summary>
    /// User email confirmation interface
    /// </summary>
    /// <remarks>
    /// Confirms user email, requiring a valid verification code
    /// </remarks>
    /// <param name="model"></param>
    /// <response code="200">User email confirmed</response>
    /// <response code="400">Verification failed</response>
    /// <response code="401">Email verification failed</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Verify([FromBody] AccountVerifyModel model)
    {
        var user = await userManager.FindByEmailAsync(Codec.Base64.Decode(model.Email));

        if (user is null || user.EmailConfirmed)
            return BadRequest(new RequestResponse("Invalid email address"));

        var result = await userManager.ConfirmEmailAsync(user, Codec.Base64.Decode(model.Token));

        if (!result.Succeeded)
            return Unauthorized(new RequestResponse("Email verification failed", 401));

        logger.Log("User's email address verified", user, TaskStatus.Success);
        await signInManager.SignInAsync(user, true);

        user.LastSignedInUTC = DateTimeOffset.UtcNow;
        user.LastVisitedUTC = DateTimeOffset.UtcNow;
        user.RegisterTimeUTC = DateTimeOffset.UtcNow;

        result = await userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return BadRequest(new RequestResponse(result.Errors.FirstOrDefault()?.Description ?? "Unknown error"));

        return Ok();
    }

    /// <summary>
    /// User login interface
    /// </summary>
    /// <remarks>
    /// Logs in to the system, requiring a valid username and password
    /// </remarks>
    /// <param name="model"></param>
    /// <response code="200">Logged in successfully</response>
    /// <response code="400">Verification failed</response>
    /// <response code="401">Incorrect username or password</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LogIn([FromBody] LoginModel model)
    {
        var user = await userManager.FindByNameAsync(model.UserName);
        user ??= await userManager.FindByEmailAsync(model.UserName);

        if (user is null)
            return Unauthorized(new RequestResponse("Incorrect username or password", 401));

        if (user.Role == Role.Banned)
            return Unauthorized(new RequestResponse("User is banned", 401));

        user.LastSignedInUTC = DateTimeOffset.UtcNow;
        user.UpdateByHttpContext(HttpContext);

        await signInManager.SignOutAsync();

        var result = await signInManager.PasswordSignInAsync(user, model.Password, true, false);

        if (!result.Succeeded)
            return Unauthorized(new RequestResponse("Incorrect username or password", 401));

        logger.Log("User successfully logged in", user, TaskStatus.Success);

        return Ok();
    }

    /// <summary>
    /// User logout interface
    /// </summary>
    /// <remarks>
    /// Logs user out of the system (User permission required)
    /// </remarks>
    /// <response code="200">Logged out successfully</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost]
    [RequireUser]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> LogOut()
    {
        await signInManager.SignOutAsync();

        return Ok();
    }

    /// <summary>
    /// User profile update interface
    /// </summary>
    /// <remarks>
    ///Updates user profile (User permission required)
    /// </remarks>
    /// <param name="model"></param>
    /// <response code="200">User profile updated successfully</response>
    /// <response code="400">Failed to verify or update user profile</response>
    /// <response code="401">Unauthorized</response>
    [HttpPut]
    [RequireUser]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update([FromBody] ProfileUpdateModel model)
    {
        var user = await userManager.GetUserAsync(User);
        var oname = user!.UserName;

        user.UpdateUserInfo(model);
        var result = await userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return BadRequest(new RequestResponse(result.Errors.FirstOrDefault()?.Description ?? "Unknown error"));

        if (oname != user.UserName)
            logger.Log($"Changed user's name: {oname} => {model.UserName}", user, TaskStatus.Success);

        return Ok();
    }

    /// <summary>
    /// User password change interface
    /// </summary>
    /// <remarks>
    /// Changes user password (User permission required)
    /// </remarks>
    /// <param name="model"></param>
    /// <response code="200">User password changed</response>
    /// <response code="400">Failed to verify or change user's password</response>
    /// <response code="401">Unauthorized</response>
    [HttpPut]
    [RequireUser]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] PasswordChangeModel model)
    {
        var user = await userManager.GetUserAsync(User);
        var result = await userManager.ChangePasswordAsync(user!, model.Old, model.New);

        if (!result.Succeeded)
            return BadRequest(new RequestResponse(result.Errors.FirstOrDefault()?.Description ?? "Unknown error"));

        logger.Log("Changed user's password", user, TaskStatus.Success);

        return Ok();
    }

    /// <summary>
    /// User email change interface
    /// </summary>
    /// <remarks>
    /// Changes user email (User permission required), Mail-URL: /confirm
    /// </remarks>
    /// <param name="model"></param>
    /// <response code="200">Mail change email sent, boolean value indicates if verification is required </response>
    /// <response code="400">Verification failed or email is already taken</response>
    /// <response code="401">Unauthorized</response>
    [HttpPut]
    [RequireUser]
    [EnableRateLimiting(nameof(RateLimiter.LimitPolicy.Register))]
    [ProducesResponseType(typeof(RequestResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangeEmail([FromBody] MailChangeModel model)
    {
        if (await userManager.FindByEmailAsync(model.NewMail) is not null)
            return BadRequest(new RequestResponse("Email address already in use"));

        var user = await userManager.GetUserAsync(User);

        if (!accountPolicy.Value.EmailConfirmationRequired)
        {
            var result = await userManager.SetEmailAsync(user!, model.NewMail);

            if (!result.Succeeded)
                return BadRequest(new RequestResponse<bool>(result.Errors.FirstOrDefault()?.Description ?? "Email change failed", false));
            return Ok(new RequestResponse<bool>("Email address changed", false, 200));
        }

         logger.Log("Sending user email change confirmation mail", user, TaskStatus.Pending);

        var token = Codec.Base64.Encode(await userManager.GenerateChangeEmailTokenAsync(user!, model.NewMail));

        if (environment.IsDevelopment())
        {
            logger.Log($"http://{HttpContext.Request.Host}/account/confirm?token={token}&email={Codec.Base64.Encode(model.NewMail)}", user, TaskStatus.Pending, LogLevel.Debug);
        }
        else
        {
            if (!mailSender.SendConfirmEmailUrl(user!.UserName, user.Email,
                $"https://{HttpContext.Request.Host}/account/confirm?token={token}&email={Codec.Base64.Encode(model.NewMail)}"))
                return BadRequest(new RequestResponse("Mail cannot be sent, please contact the administrator"));
        }

        return Ok(new RequestResponse<bool>("Email change pending", true, 200));
    }

    /// <summary>
    /// User email change confirmation interface
    /// </summary>
    /// <remarks>
    /// Confirms user email change (User permission required)
    /// </remarks>
    /// <param name="model"></param>
    /// <response code="200">Email changed successfully</response>
    /// <response code="400">Failed to verify or change user's email</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [HttpPost]
    [RequireUser]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MailChangeConfirm([FromBody] AccountVerifyModel model)
    {
        var user = await userManager.GetUserAsync(User);
        var result = await userManager.ChangeEmailAsync(user!, Codec.Base64.Decode(model.Email), Codec.Base64.Decode(model.Token));

        if (!result.Succeeded)
            return BadRequest(new RequestResponse("Invalid token or email"));

        logger.Log("Changed user's email address", user, TaskStatus.Success);

        return Ok();
    }

    /// <summary>
    /// User profile information interface
    /// </summary>
    /// <remarks>
    /// Gets user profile information (User permission required)
    /// </remarks>
    /// <response code="200">Profile information</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet]
    [RequireUser]
    [ProducesResponseType(typeof(ProfileUserInfoModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Profile()
    {
        var user = await userManager.GetUserAsync(User);

        return Ok(ProfileUserInfoModel.FromUserInfo(user!));
    }

    /// <summary>
    /// User avatar change interface
    /// </summary>
    /// <remarks>
    /// Set user avatar (User permission required)
    /// </remarks>
    /// <response code="200">Avatar changed successfully</response>
    /// <response code="400">Bad request</response>
    /// <response code="401">Unauthorized</response>
    [HttpPut]
    [RequireUser]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Avatar(IFormFile file, CancellationToken token)
    {
        if (file.Length == 0)
            return BadRequest(new RequestResponse("File is invalid"));

        if (file.Length > 3 * 1024 * 1024)
            return BadRequest(new RequestResponse("File is too large"));

        var user = await userManager.GetUserAsync(User);

        if (user!.AvatarHash is not null)
            await fileService.DeleteFileByHash(user.AvatarHash, token);

        var avatar = await fileService.CreateOrUpdateFile(file, "avatar", token);

        if (avatar is null)
            return BadRequest(new RequestResponse("Failed create avatar"));

        user.AvatarHash = avatar.Hash;
        var result = await userManager.UpdateAsync(user);

        if (result != IdentityResult.Success)
            return BadRequest(new RequestResponse("Failed to set avatar"));

        logger.Log($"Changed user's avatar：[{avatar.Hash[..8]}]", user, TaskStatus.Success);

        return Ok(avatar.Url());
    }
}
