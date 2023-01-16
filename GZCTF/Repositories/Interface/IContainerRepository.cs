namespace CTFServer.Repositories.Interface;

public interface IContainerRepository : IRepository
{
    /// <summary>
    /// Get all containers
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<List<Container>> GetContainers(CancellationToken token = default);

    /// <summary>
    /// Get all containers to be destroyed
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<List<Container>> GetDyingContainers(CancellationToken token = default);

    /// <summary>
    /// Remove the specified container (the container has been destroyed)
    /// </summary>
    /// <param name="container">容器对象</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task RemoveContainer(Container container, CancellationToken token = default);
}