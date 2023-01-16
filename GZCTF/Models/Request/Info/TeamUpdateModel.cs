using System.ComponentModel.DataAnnotations;

namespace CTFServer.Models.Request.Info;

/// <summary>
/// Team information update
/// </summary>
public class TeamUpdateModel
{
    /// <summary>
    /// Team name
    /// </summary>
    [MaxLength(15, ErrorMessage = "Team name too long")]
    public string? Name { get; set; } = string.Empty;

    /// <summary>
    /// Team bio
    /// </summary>
    [MaxLength(31, ErrorMessage = "Bio too long")]
    public string? Bio { get; set; }
}