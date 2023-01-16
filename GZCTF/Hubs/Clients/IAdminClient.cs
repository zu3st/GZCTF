using CTFServer.Models.Request.Admin;

namespace CTFServer.Hubs.Client;

public interface IAdminClient
{
    /// <summary>
    /// Received global log information
    /// </summary>
    public Task ReceivedLog(LogMessageModel log);
}