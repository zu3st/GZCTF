using CTFServer.Models.Request.Admin;
using CTFServer.Models.Request.Game;

namespace CTFServer.Repositories.Interface;

public interface IParticipationRepository : IRepository
{
    /// <summary>
    /// Get the number of participations in a game
    /// </summary>
    /// <param name="game">比赛对象</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<int> GetParticipationCount(Game game, CancellationToken token = default);

    /// <summary>
    /// Get all participations of a game
    /// </summary>
    /// <param name="game"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Participation[]> GetParticipations(Game game, CancellationToken token = default);

    /// <summary>
    /// Get all writeups of a game
    /// </summary>
    /// <param name="game"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<WriteupInfoModel[]> GetWriteups(Game game, CancellationToken token = default);

    /// <summary>
    /// Ensure that an instance exists for a participation
    /// </summary>
    /// <param name="part"></param>
    /// <param name="game"></param>
    /// <param name="token"></param>
    /// <returns>Availability of the instance</returns>
    public Task<bool> EnsureInstances(Participation part, Game game, CancellationToken token = default);

    /// <summary>
    /// Check for repeated participation
    /// </summary>
    /// <param name="user">User to check</param>
    /// <param name="game">Game object</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<bool> CheckRepeatParticipation(UserInfo user, Game game, CancellationToken token = default);

    /// <summary>
    /// Remove all participation objects of a user by game
    /// </summary>
    /// <param name="user">Participating user</param>
    /// <param name="game">Game object</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task RemoveUserParticipations(UserInfo user, Game game, CancellationToken token = default);

    /// <summary>
    /// Remove all participation objects of a user by team
    /// </summary>
    /// <param name="user">Participating user</param>
    /// <param name="team">Team object</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task RemoveUserParticipations(UserInfo user, Team team, CancellationToken token = default);

    /// <summary>
    /// Change participation status by id 
    /// </summary>
    /// <param name="id">Participant id</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Participation?> GetParticipationById(int id, CancellationToken token = default);

    /// <summary>
    /// Get participation object and corresponding question list by user and game
    /// </summary>
    /// <param name="user">User object</param>
    /// <param name="game">Game object</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Participation?> GetParticipation(UserInfo user, Game game, CancellationToken token = default);

    /// <summary>
    /// Get participation object and corresponding question list by team and game
    /// </summary>
    /// <param name="team">Team object</param>
    /// <param name="game">Game object</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Participation?> GetParticipation(Team team, Game game, CancellationToken token = default);

    /// <summary>
    /// Remove participation
    /// </summary>
    /// <param name="part">Participation object</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task RemoveParticipation(Participation part, CancellationToken token = default);

    /// <summary>
    /// Update participation status
    /// </summary>
    /// <param name="part">Participation object</param>
    /// <param name="status">New status</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task UpdateParticipationStatus(Participation part, ParticipationStatus status, CancellationToken token = default);
}