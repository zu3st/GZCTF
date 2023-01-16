namespace CTFServer.Repositories.Interface;

public interface IGameNoticeRepository : IRepository
{
    /// <summary>
    /// Add a game notice
    /// </summary>
    /// <param name="notice">通知</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<GameNotice> AddNotice(GameNotice notice, CancellationToken token = default);

    /// <summary>
    /// Get all game notices
    /// </summary>
    /// <param name="gameId">game id</param>
    /// <param name="count">Number of notices to return</param>
    /// <param name="skip">Number of notices to skip</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<GameNotice[]> GetNotices(int gameId, int count = 20, int skip = 0, CancellationToken token = default);

    /// <summary>
    /// Get game normal notices (editable)
    /// </summary>
    /// <param name="gameId">Game id</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<GameNotice[]> GetNormalNotices(int gameId, CancellationToken token = default);

    /// <summary>
    /// Update game notice
    /// </summary>
    /// <param name="notice"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<GameNotice> UpdateNotice(GameNotice notice, CancellationToken token = default);

    /// <summary>
    /// Get game notice by id
    /// </summary>
    /// <param name="gameId">Game id</param>
    /// <param name="noticeId">Notice id</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<GameNotice?> GetNoticeById(int gameId, int noticeId, CancellationToken token = default);

    /// <summary>
    /// Remove game notice
    /// </summary>
    /// <param name="notice">Notice to remove</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task RemoveNotice(GameNotice notice, CancellationToken token = default);
}