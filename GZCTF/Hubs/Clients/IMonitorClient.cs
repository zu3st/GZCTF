namespace CTFServer.Hubs.Clients;

public interface IMonitorClient
{
    /// <summary>
    /// Received game event information
    /// </summary>
    public Task ReceivedGameEvent(GameEvent gameEvent);

    /// <summary>
    /// Received game submission information
    /// </summary>
    public Task ReceivedSubmissions(Submission submission);
}