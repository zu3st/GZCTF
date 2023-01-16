using System.Text.Json.Serialization;

namespace CTFServer.Models.Request.Info;

/// <summary>
/// Team user information
/// </summary>
public class TeamUserInfoModel
{
    /// <summary>
    /// User id
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Username
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Bio
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    /// Avatar url
    /// </summary>
    public string? Avatar { get; set; }

    /// <summary>
    /// Whether the user is the captain
    /// </summary>
    public bool Captain { get; set; }

    /// <summary>
    /// Real name
    /// </summary>
    [JsonIgnore]
    public string? RealName { get; set; }

    /// <summary>
    /// Matriculation number
    /// </summary>
    [JsonIgnore]
    public string? StudentNumber { get; set; }
}