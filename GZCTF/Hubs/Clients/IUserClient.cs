namespace CTFServer.Hubs.Clients;

public interface IUserClient
{
    /// <summary>
    /// Received game notice
    /// </summary>
    public Task ReceivedGameNotice(GameNotice notice);
}