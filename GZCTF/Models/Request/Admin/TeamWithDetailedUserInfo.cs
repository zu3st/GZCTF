using CTFServer.Models.Request.Account;

namespace CTFServer.Models.Request.Admin;

/// <summary>
/// Detailed information of the team for review (Admin)
/// </summary>
public class TeamWithDetailedUserInfo
{
    /// <summary>
    /// Team Id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Team name
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Team bio
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    /// Team avatar
    /// </summary>
    public string? Avatar { get; set; }

    /// <summary>
    /// Whether the team is locked
    /// </summary>
    public bool Locked { get; set; }

    /// <summary>
    /// Captain Id
    /// </summary>
    public string CaptainId { get; set; } = string.Empty;

    /// <summary>
    /// Team members
    /// </summary>
    public ProfileUserInfoModel[]? Members { get; set; }

    internal static TeamWithDetailedUserInfo FromTeam(Team team)
        => new()
        {
            Id = team.Id,
            Name = team.Name,
            Bio = team.Bio,
            Avatar = team.AvatarUrl,
            Locked = team.Locked,
            CaptainId = team.CaptainId,
            Members = team.Members.Select(ProfileUserInfoModel.FromUserInfo).ToArray()
        };
}
