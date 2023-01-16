using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace CTFServer.Utils;

public class HubHelper
{
    /// <summary>
    /// Whether the current request has permission
    /// </summary>
    /// <param name="context">Current request</param>
    /// <param name="privilege">Permission</param>
    /// <returns></returns>
    public static async Task<bool> HasPrivilege(HttpContext context, Role privilege)
    {
        var dbContext = context.RequestServices.GetRequiredService<AppDbContext>();
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (dbContext is null || userId is null)
            return false;

        var currentUser = await dbContext.Users.FirstOrDefaultAsync(i => i.Id == userId);

        var env = context.RequestServices.GetRequiredService<IHostEnvironment>();

        return currentUser is not null && currentUser.Role >= privilege;
    }

    /// <summary>
    /// Whether the current request has <see cref="Role.Admin"/> permission
    /// </summary>
    /// <param name="context">Current request</param>
    /// <returns></returns>
    public static Task<bool> HasAdmin(HttpContext context)
        => HasPrivilege(context, Role.Admin);

    /// <summary>
    /// Whether the current request has permission greater than or equal to<see cref="Role.Monitor"/>
    /// </summary>
    /// <param name="context">Current request</param>
    /// <returns></returns>
    public static Task<bool> HasMonitor(HttpContext context)
        => HasPrivilege(context, Role.Monitor);

    /// <summary>
    /// Whether the current request has permission greater than or equal to <see cref="Role.User"/>
    /// </summary>
    /// <param name="context">Current request</param>
    /// <returns></returns>
    public static Task<bool> HasUser(HttpContext context)
        => HasPrivilege(context, Role.User);
}