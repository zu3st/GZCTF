using CTFServer.Models.Request.Info;

namespace CTFServer.Repositories.Interface;

public interface ITeamRepository : IRepository
{
    /// <summary>
    /// Create a team
    /// </summary>
    /// <param name="model">Team information</param>
    /// <param name="user">User creating the team</param>
    /// <param name="token"></param>
    /// <returns>Team object</returns>
    public Task<Team?> CreateTeam(TeamUpdateModel model, UserInfo user, CancellationToken token = default);

    /// <summary>
    /// Get team by id
    /// </summary>
    /// <param name="id">Team id</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Team?> GetTeamById(int id, CancellationToken token = default);

    /// <summary>
    /// Get teams of a user
    /// </summary>
    /// <param name="user">User to get teams from</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Team[]> GetUserTeams(UserInfo user, CancellationToken token = default);

    /// <summary>
    /// Check if a user is a captain of any team
    /// </summary>
    /// <param name="user">User to check</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<bool> CheckIsCaptain(UserInfo user, CancellationToken token = default);

    /// <summary>
    /// Search teams by search hint
    /// </summary>
    /// <param name="hint">Search hint</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Team[]> SearchTeams(string hint, CancellationToken token = default);

    /// <summary>
    /// Get teams by page
    /// </summary>
    /// <param name="count">Number of teams to get</param>
    /// <param name="skip">Number of teams to skip</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Team[]> GetTeams(int count = 100, int skip = 0, CancellationToken token = default);

    /// <summary>
    /// Whether there is an active game of the team, if there is, no changes can be made to the team
    /// </summary>
    /// <param name="team"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<bool> AnyActiveGame(Team team, CancellationToken token = default);

    /// <summary>
    /// Transfer team ownership
    /// </summary>
    /// <param name="team">Team to transfer</param>
    /// <param name="user">New captain</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task Transfer(Team team, UserInfo user, CancellationToken token = default);

    /// <summary>
    /// Verify invite token
    /// </summary>
    /// <param name="id">Team id</param>
    /// <param name="inviteToken">Invite token</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<bool> VerifyToken(int id, string inviteToken, CancellationToken token = default);

    /// <summary>
    /// Delete a team
    /// </summary>
    /// <param name="team">Team to delete</param>
    /// <param name="token"></param>
    /// <returns>Deleted team</returns>
    public Task DeleteTeam(Team team, CancellationToken token = default);
}
