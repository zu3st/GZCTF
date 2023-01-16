using System;
using System.Globalization;
using System.Net;
using System.Threading.RateLimiting;
using CTFServer.Utils;
using Microsoft.AspNetCore.RateLimiting;

namespace CTFServer.Middlewares;

/// <summary>
/// Rate Limiter
/// </summary>
public class RateLimiter
{
    public enum LimitPolicy
    {
        /// <summary>
        /// Concurrency limit
        /// </summary>
        Concurrency,

        /// <summary>
        /// Register request limit
        /// </summary>
        Register,

        /// <summary>
        /// Container operation limit
        /// </summary>
        Container,

        /// <summary>
        /// Submit request limit
        /// </summary>
        Submit
    }

    public static RateLimiterOptions GetRateLimiterOptions()
        => new RateLimiterOptions()
        {
            RejectionStatusCode = StatusCodes.Status429TooManyRequests,
            GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, IPAddress>(context =>
            {
                IPAddress? remoteIPaddress = context?.Connection?.RemoteIpAddress;

                if (remoteIPaddress is not null && !IPAddress.IsLoopback(remoteIPaddress))
                {
                    return RateLimitPartition.GetSlidingWindowLimiter<IPAddress>(remoteIPaddress, key => new()
                    {
                        PermitLimit = 150,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 60,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        SegmentsPerWindow = 6,
                    });
                }
                else
                {
                    return RateLimitPartition.GetNoLimiter<IPAddress>(IPAddress.Loopback);
                }
            }),
            OnRejected = (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                    ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
                }

                context?.HttpContext?.RequestServices?
                    .GetService<ILoggerFactory>()?
                    .CreateLogger<RateLimiter>()
                    .Log($"Too many requests: {context.HttpContext.Request.Path}",
                        context.HttpContext, TaskStatus.Denied, LogLevel.Debug);

                return new ValueTask();
            }
        }
        .AddConcurrencyLimiter(nameof(LimitPolicy.Concurrency), options =>
        {
            options.PermitLimit = 1;
            options.QueueLimit = 5;
        })
        .AddFixedWindowLimiter(nameof(LimitPolicy.Register), options =>
        {
            options.PermitLimit = 10;
            options.Window = TimeSpan.FromSeconds(150);
        })
        .AddTokenBucketLimiter(nameof(LimitPolicy.Container), options =>
        {
            options.TokenLimit = 4;
            options.TokensPerPeriod = 2;
            options.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
        })
        .AddTokenBucketLimiter(nameof(LimitPolicy.Submit), options =>
        {
            options.TokenLimit = 3;
            options.TokensPerPeriod = 1;
            options.ReplenishmentPeriod = TimeSpan.FromSeconds(20);
        });
}

public static class RateLimiterExtensions
{
    public static IApplicationBuilder UseConfiguredRateLimiter(this IApplicationBuilder builder)
        => builder.UseRateLimiter(RateLimiter.GetRateLimiterOptions());
}

