using CTFServer.Models.Internal;
using CTFServer.Utils;

namespace CTFServer.Repositories.Interface;

public interface IInstanceRepository : IRepository
{
    /// <summary>
    /// Get or create instance
    /// </summary>
    /// <param name="team">Team</param>
    /// <param name="challengeId">Challenge id</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Instance?> GetInstance(Participation team, int challengeId, CancellationToken token = default);

    /// <summary>
    /// Verify answer
    /// </summary>
    /// <param name="submission">Submission</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<(SubmissionType, AnswerResult)> VerifyAnswer(Submission submission, CancellationToken token = default);

    /// <summary>
    /// Get challenge instances
    /// </summary>
    /// <param name="challenge">Challenge</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Instance[]> GetInstances(Challenge challenge, CancellationToken token = default);

    /// <summary>
    /// Check for cheating
    /// </summary>
    /// <param name="submission">Submission</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<CheatCheckInfo> CheckCheat(Submission submission, CancellationToken token = default);

    /// <summary>
    /// Create container instance
    /// </summary>
    /// <param name="instance">instance object</param>
    /// <param name="team">team object</param>
    /// <param name="containerLimit">Concurrent container limit</param>
    /// <param name="user">User object</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<TaskResult<Container>> CreateContainer(Instance instance, Team team, UserInfo user, int containerLimit = 3, CancellationToken token = default);

    /// <summary>
    /// Destroy container instance
    /// </summary>
    /// <param name="container">Container instance object</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<bool> DestroyContainer(Container container, CancellationToken token = default);

    /// <summary>
    /// Prolong container
    /// </summary>
    /// <param name="container">Container instance object</param>
    /// <param name="time">Extension time</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task ProlongContainer(Container container, TimeSpan time, CancellationToken token = default);
}