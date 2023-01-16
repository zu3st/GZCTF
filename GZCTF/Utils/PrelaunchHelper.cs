using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Serilog;

namespace CTFServer.Utils;

public static class PrelaunchHelper
{
    public async static Task RunPrelaunchWork(this WebApplication app)
    {
        using var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

        var context = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var cache = serviceScope.ServiceProvider.GetRequiredService<IDistributedCache>();

        if (!context.Database.IsInMemory())
            await context.Database.MigrateAsync();

        await context.Database.EnsureCreatedAsync();

        if (!await context.Posts.AnyAsync())
        {
            await context.Posts.AddAsync(new()
            {
                UpdateTimeUTC = DateTimeOffset.UtcNow,
                Title = "Welcome to GZ::CTF!",
                Summary = "An open-source CTF platform.",
                Content = "The project is based on the AGPL-3.0 license and open-sourced at [GZTimeWalker/GZCTF](https://github.com/GZTimeWalker/GZCTF)。"
            });

            await context.SaveChangesAsync();
        }

        if (app.Environment.IsDevelopment() || app.Configuration.GetSection("ADMIN_PASSWORD").Exists())
        {
            var usermanager = serviceScope.ServiceProvider.GetRequiredService<UserManager<UserInfo>>();
            var admin = await usermanager.FindByNameAsync("Admin");
            var password = app.Environment.IsDevelopment() ? "Admin@2022" :
                app.Configuration.GetValue<string>("ADMIN_PASSWORD");

            if (admin is null && password is not null)
            {
                admin = new UserInfo
                {
                    UserName = "Admin",
                    Email = "admin@gzti.me",
                    Role = CTFServer.Role.Admin,
                    EmailConfirmed = true,
                    RegisterTimeUTC = DateTimeOffset.UtcNow
                };
                await usermanager.CreateAsync(admin, password);
            }
        }

        if (!cache.CacheCheck())
            Program.ExitWithFatalMessage("Cache configuration is invalid. Please check the configuration of the RedisCache field. If you are not using Redis, please leave the configuration item blank.");
    }

    public static bool CacheCheck(this IDistributedCache cache)
    {
        try
        {
            cache.SetString("_ValidCheck", "GZCTF");
            return cache.GetString("_ValidCheck") == "GZCTF";
        }
        catch
        {
            return false;
        }
    }
}
