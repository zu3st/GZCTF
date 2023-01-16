using CTFServer.Models.Request.Game;

namespace CTFServer.Repositories.Interface;

public interface IGameRepository : IRepository
{
    /// <summary>
    /// Get basic information of the specified number of game objects
    /// </summary>
    /// <param name="count"></param>
    /// <param name="skip"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<BasicGameInfoModel[]> GetBasicGameInfo(int count = 10, int skip = 0, CancellationToken token = default);

    /// <summary>
    /// Get the specified number of game objects
    /// </summary>
    /// <param name="count"></param>
    /// <param name="skip"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Game[]> GetGames(int count = 10, int skip = 0, CancellationToken token = default);

    /// <summary>
    /// Get game object by id
    /// </summary>
    /// <param name="id">比赛Id</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Game?> GetGameById(int id, CancellationToken token = default);

    /// <summary>
    /// Create a game object
    /// </summary>
    /// <param name="game">Game object</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Game?> CreateGame(Game game, CancellationToken token = default);

    /// <summary>
    /// Get team token
    /// </summary>
    /// <param name="game">Game object</param>
    /// <param name="team">Team to get token</param>
    /// <returns></returns>
    public string GetToken(Game game, Team team);

    /// <summary>
    /// Get scoreboard
    /// </summary>
    /// <param name="game">Game object</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<ScoreboardModel> GetScoreboard(Game game, CancellationToken token = default);

    /// <summary>
    /// Delete game
    /// </summary>
    /// <param name="game">Game object</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task DeleteGame(Game game, CancellationToken token = default);

    /// <summary>
    /// Flush scoreboard cache
    /// </summary>
    /// <param name="gameId">比赛Id</param>
    public void FlushScoreboardCache(int gameId);

    /// <summary>
    /// Flush game info cache
    /// </summary>
    public void FlushGameInfoCache();
}
