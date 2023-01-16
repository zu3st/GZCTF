using CTFServer.Models.Internal;

namespace CTFServer.Models.Request.Admin;

/// <summary>
/// Configurations edit
/// </summary>
public class ConfigEditModel
{
    /// <summary>
    /// Account policy
    /// </summary>
    public AccountPolicy? AccountPolicy { get; set; }

    /// <summary>
    /// Global configuration
    /// </summary>
    public GlobalConfig? GlobalConfig { get; set; }
}