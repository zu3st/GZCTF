namespace CTFServer.Repositories.Interface;

public interface IGameEventRepository : IRepository
{
    /// <summary>
    /// Add an event
    /// </summary>
    /// <param name="event">Event</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<GameEvent> AddEvent(GameEvent @event, CancellationToken token = default);

    /// <summary>
    /// Get all events
    /// </summary>
    /// <param name="gameId">Game Id</param>
    /// <param name="hideContainer">Hide container events</param>
    /// <param name="count">Number of events to return</param>
    /// <param name="skip">Number of events to skip</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<GameEvent[]> GetEvents(int gameId, bool hideContainer = false, int count = 50, int skip = 0, CancellationToken token = default);
}