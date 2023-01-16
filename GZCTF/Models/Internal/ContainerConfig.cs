namespace CTFServer.Models.Internal;

public class ContainerConfig
{
    /// <summary>
    /// Container image
    /// </summary>
    public string Image { get; set; } = string.Empty;

    /// <summary>
    /// Team id
    /// </summary>
    public string TeamId { get; set; } = string.Empty;

    /// <summary>
    /// User id
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Exposed container port
    /// </summary>
    public int ExposedPort { get; set; }

    /// <summary>
    /// Flag text
    /// </summary>
    public string? Flag { get; set; } = string.Empty;

    /// <summary>
    /// Whether the container is privileged
    /// </summary>
    public bool PrivilegedContainer { get; set; } = false;

    /// <summary>
    /// Memory limit (MB)
    /// </summary>
    public int MemoryLimit { get; set; } = 64;

    /// <summary>
    /// CPU count limit
    /// </summary>
    public int CPUCount { get; set; } = 1;

    /// <summary>
    /// Storage limit (MB)
    /// </summary>
    public int StorageLimit { get; set; } = 256;
}
