namespace CTFServer.Services.Interface;

public interface IConfigService
{
    /// <summary>
    /// Save configuration
    /// </summary>
    /// <typeparam name="T">Configuration type</typeparam>
    /// <param name="config">Configuration object</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task SaveConfig<T>(T config, CancellationToken token = default) where T : class;

    /// <summary>
    /// Save configuration
    /// </summary>
    /// <param name="type">Configuration type</param>
    /// <param name="value">Configuration value</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task SaveConfig(Type type, object? value, CancellationToken token = default);

    /// <summary>
    /// Reload configuration
    /// </summary>
    public void ReloadConfig();
}