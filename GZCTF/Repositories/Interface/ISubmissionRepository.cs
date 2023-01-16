namespace CTFServer.Repositories.Interface;

public interface ISubmissionRepository : IRepository
{
    /// <summary>
    /// Get game submissions by page, in descending order of submission time
    /// </summary>
    /// <param name="game">Game to get submissions from</param>
    /// <param name="type">Submission type</param>
    /// <param name="count">Number of submissions to get</param>
    /// <param name="skip">Number of submissions to skip</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Submission[]> GetSubmissions(Game game, AnswerResult? type = null, int count = 100, int skip = 0, CancellationToken token = default);

    /// <summary>
    /// Get challenge submissions by page, in descending order of submission time
    /// </summary>
    /// <param name="challenge">Challenge to get submissions from</param>
    /// <param name="type">Submission type</param>
    /// <param name="count">Number of submissions to get</param>
    /// <param name="skip">Number of submissions to skip</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Submission[]> GetSubmissions(Challenge challenge, AnswerResult? type = null, int count = 100, int skip = 0, CancellationToken token = default);

    /// <summary>
    /// Submit a submission to the queue
    /// </summary>
    /// <param name="submission"></param>
    public Task SendSubmission(Submission submission);

    /// <summary>
    /// Get team submissions by page, in descending order of submission time
    /// </summary>
    /// <param name="team">Team to get submissions from</param>
    /// <param name="type">Submission type</param>
    /// <param name="count">Number of submissions to get</param>
    /// <param name="skip">Number of submissions to skip</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Submission[]> GetSubmissions(Participation team, AnswerResult? type = null, int count = 100, int skip = 0, CancellationToken token = default);

    /// <summary>
    /// Add a submission
    /// </summary>
    /// <param name="submission">提交对象</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Submission> AddSubmission(Submission submission, CancellationToken token = default);

    /// <summary>
    /// Get unchecked flags
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Submission[]> GetUncheckedFlags(CancellationToken token = default);

    /// <summary>
    /// Get a submission
    /// </summary>
    /// <param name="gameId">Game Id</param>
    /// <param name="challengeId">Challenge Id</param>
    /// <param name="userId">User Id</param>
    /// <param name="submitId">Submission Id</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Submission?> GetSubmission(int gameId, int challengeId, string userId, int submitId, CancellationToken token = default);
}