using CTFServer.Models.Internal;

namespace CTFServer.Services.Interface;

public interface IContainerService
{
    /// <summary>
    /// Create a container
    /// </summary>
    /// <param name="config">Container configuration</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Container?> CreateContainerAsync(ContainerConfig config, CancellationToken token = default);

    /// <summary>
    /// Destroy a container
    /// </summary>
    /// <param name="container">Container to destroy</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task DestroyContainerAsync(Container container, CancellationToken token = default);
}
