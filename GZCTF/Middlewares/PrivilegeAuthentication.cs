using System.Security.Claims;
using CTFServer.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace CTFServer.Middlewares;

/// <summary>
/// Require privilege to access
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequirePrivilegeAttribute : Attribute, IAsyncAuthorizationFilter
{
    /// <summary>
    /// Required privilege
    /// </summary>
    private readonly Role RequiredPrivilege;

    public RequirePrivilegeAttribute(Role privilege)
        => RequiredPrivilege = privilege;

    public static IActionResult GetResult(string msg, int code)
        => new JsonResult(new RequestResponse(msg, code)) { StatusCode = code };

    public static IActionResult RequireLoginResult => GetResult("Please login first", StatusCodes.Status401Unauthorized);
    public static IActionResult ForbiddenResult => GetResult("Forbidden", StatusCodes.Status403Forbidden);

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<RequirePrivilegeAttribute>>();
        var dbcontext = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();

        UserInfo? user = null;

        if (context.HttpContext.User.Identity?.IsAuthenticated is true)
            user = await dbcontext.Users.SingleOrDefaultAsync(u => u.Id ==
                context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

        if (user is null)
        {
            context.Result = RequireLoginResult;
            return;
        }

        if (DateTimeOffset.UtcNow - user.LastVisitedUTC > TimeSpan.FromSeconds(5))
        {
            user.UpdateByHttpContext(context.HttpContext);
            await dbcontext.SaveChangesAsync(); // Avoid to update ConcurrencyStamp
        }

        if (user.Role < RequiredPrivilege)
        {
            if (RequiredPrivilege > Role.User)
                logger.Log($"Unauthorized access: {context.HttpContext.Request.Path}", user, TaskStatus.Denied);

            context.Result = ForbiddenResult;
        }
    }
}

/// <summary>
/// Require logged in user
/// </summary>
public class RequireUserAttribute : RequirePrivilegeAttribute
{
    public RequireUserAttribute() : base(Role.User)
    {
    }
}

/// <summary>
/// Require Monitor privilege
/// </summary>
public class RequireMonitorAttribute : RequirePrivilegeAttribute
{
    public RequireMonitorAttribute() : base(Role.Monitor)
    {
    }
}

/// <summary>
/// Require Admin privilege
/// </summary>
public class RequireAdminAttribute : RequirePrivilegeAttribute
{
    public RequireAdminAttribute() : base(Role.Admin)
    {
    }
}