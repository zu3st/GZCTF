using CTFServer.Models.Request.Edit;

namespace CTFServer.Repositories.Interface;

public interface IChallengeRepository : IRepository
{
    /// <summary>
    /// Create a challenge object
    /// </summary>
    /// <param name="game">Game object</param>
    /// <param name="challenge">Challenge object</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Challenge> CreateChallenge(Game game, Challenge challenge, CancellationToken token = default);

    /// <summary>
    /// Remove a challenge object
    /// </summary>
    /// <param name="challenge">Challenge object</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task RemoveChallenge(Challenge challenge, CancellationToken token = default);

    /// <summary>
    /// Get all challenges
    /// </summary>
    /// <param name="gameId">Game Id</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Challenge[]> GetChallenges(int gameId, CancellationToken token = default);

    /// <summary>
    /// Get a challenge
    /// </summary>
    /// <param name="gameId">Game id</param>
    /// <param name="id">Challenge id</param>
    /// <param name="withFlag">Whether to return flag</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Challenge?> GetChallenge(int gameId, int id, bool withFlag = false, CancellationToken token = default);

    /// <summary>
    /// Add Flag
    /// </summary>
    /// <param name="challenge">Challenge object</param>
    /// <param name="model">Flag information</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task AddFlags(Challenge challenge, FlagCreateModel[] model, CancellationToken token = default);

    /// <summary>
    /// Ensure that an instance exists for a challenge
    /// </summary>
    /// <param name="challenge"></param>
    /// <param name="game"></param>
    /// <param name="token"></param>
    /// <returns>Availability of the instance</returns>
    public Task<bool> EnsureInstances(Challenge challenge, Game game, CancellationToken token = default);

    /// <summary>
    /// Update attachment
    /// </summary>
    /// <param name="challenge">Challenge object</param>
    /// <param name="model">Attachment information</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task UpdateAttachment(Challenge challenge, AttachmentCreateModel model, CancellationToken token = default);

    /// <summary>
    /// Remove Flag, make sure the Flags field is loaded
    /// </summary>
    /// <param name="challenge">Challenge object</param>
    /// <param name="flagId">flag ID</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<TaskStatus> RemoveFlag(Challenge challenge, int flagId, CancellationToken token = default);

    /// <summary>
    /// Verify static answer (multiple answers are possible)
    /// </summary>
    /// <param name="challenge">Challenge object</param>
    /// <param name="flag">Flag text</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<bool> VerifyStaticAnswer(Challenge challenge, string flag, CancellationToken token = default);
}