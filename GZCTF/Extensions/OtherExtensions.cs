using CTFServer.Utils;

namespace CTFServer.Extensions;

public static class ListExtensions
{
    public static int GetSetHashCode<T>(this IList<T> list)
        => list.Count + list.Distinct().Aggregate(0, (x, y) => x.GetHashCode() ^ y?.GetHashCode() ?? 0xdead);
}

public static class IQueryableExtensions
{
    /// <summary>
    /// If count is greater than 0, then only get part of it
    /// Warn: May cause malicious parameter injection to get all data
    /// </summary>
    /// <returns></returns>
    public static IQueryable<T> TakeAllIfZero<T>(this IQueryable<T> items, int count = 100, int skip = 0)
        => count switch
        {
            > 0 => items.Skip(skip).Take(count),
            _ => items
        };
}

public static class ArrayExtensions
{
    public static ArrayResponse<T> ToResponse<T>(this IEnumerable<T> array, int? tot = null) where T : class
        => array switch
        {
            null => new(Array.Empty<T>()),
            T[] arr => new(arr, tot),
            _ => new(array.ToArray(), tot)
        };
}