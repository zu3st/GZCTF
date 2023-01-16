using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CTFServer.Models.Request.Info;

namespace CTFServer.Models;

public class Team
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Team name
    /// </summary>
    [Required]
    [MaxLength(15)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Team bio
    /// </summary>
    [MaxLength(31)]
    public string? Bio { get; set; } = string.Empty;

    /// <summary>
    /// Team avatar hash
    /// </summary>
    [MaxLength(64)]
    public string? AvatarHash { get; set; }

    /// <summary>
    /// Whether the team is locked
    /// </summary>
    public bool Locked { get; set; } = false;

    /// <summary>
    /// Invite token
    /// </summary>
    public string InviteToken { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Invite code
    /// </summary>
    [NotMapped]
    public string InviteCode => $"{Name}:{Id}:{InviteToken}";

    #region Db Relationship

    /// <summary>
    ///  Captain User Id
    /// </summary>
    public string CaptainId { get; set; } = string.Empty;

    /// <summary>
    /// Captain of the team
    /// </summary>
    public UserInfo? Captain { get; set; }

    /// <summary>
    /// Team participations
    /// </summary>
    public List<Participation> Participations { get; set; } = new();

    /// <summary>
    /// Games participated in
    /// </summary>
    public HashSet<Game>? Games { get; set; }

    /// <summary>
    /// Team members
    /// </summary>
    public HashSet<UserInfo> Members { get; set; } = new();

    #endregion Db Relationship

    [NotMapped]
    public string? AvatarUrl => AvatarHash is null ? null : $"/assets/{AvatarHash}/avatar";

    public void UpdateInviteToken() => InviteToken = Guid.NewGuid().ToString("N");

    internal void UpdateInfo(TeamUpdateModel model)
    {
        Name = string.IsNullOrEmpty(model.Name) ? Name : model.Name;
        Bio = model.Bio;
    }
}