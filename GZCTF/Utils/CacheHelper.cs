using MemoryPack;
using Microsoft.Extensions.Caching.Distributed;

namespace CTFServer.Utils;

public static class CacheHelper
{
    public static async Task<T> GetOrCreateAsync<T, L>(this IDistributedCache cache,
        ILogger<L> logger,
        string key,
        Func<DistributedCacheEntryOptions, Task<T>> func,
        CancellationToken token = default)
        where T : class
    {
        var value = await cache.GetAsync(key, token);
        T? result = default;

        if (value is not null)
        {
            try
            {
                result = MemoryPackSerializer.Deserialize<T>(value);
            }
            catch
            { }
            if (result is not null)
                return result;
        }

        var cacheOptions = new DistributedCacheEntryOptions();
        result = await func(cacheOptions);
        var bytes = MemoryPackSerializer.Serialize(result);

        await cache.SetAsync(key, bytes, cacheOptions, token);
        logger.SystemLog($"Rebuilt cache: {key} @ {bytes.Length} bytes", TaskStatus.Success, LogLevel.Debug);

        return result;
    }
}

/// <summary>
/// Cache item keys
/// </summary>
public static class CacheKey
{
    /// <summary>
    /// Scoreboard cache
    /// </summary>
    public static string ScoreBoard(int id) => $"_ScoreBoard_{id}";

    /// <summary>
    /// Game notice cache
    /// </summary>
    public static string GameNotice(int id) => $"_GameNotice_{id}";

    /// <summary>
    /// Basic game information cache
    /// </summary>
    public const string BasicGameInfo = "_BasicGameInfo";

    /// <summary>
    /// Posts cache
    /// </summary>
    public const string Posts = "_Posts";
}
