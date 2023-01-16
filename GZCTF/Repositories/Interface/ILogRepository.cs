using CTFServer.Models.Request.Admin;

namespace CTFServer.Repositories.Interface;

public interface ILogRepository : IRepository
{
    /// <summary>
    /// Get specified number of log entries for a specific level
    /// </summary>
    /// <param name="skip">Number of log entries to skip</param>
    /// <param name="count">Number of log entries to get</param>
    /// <param name="level">Log level</param>
    /// <param name="token">Operation cancellation token</param>
    /// <returns>No more than <paramref name="count"/> log entries</returns>
    public Task<LogMessageModel[]> GetLogs(int skip, int count, string? level, CancellationToken token);
}