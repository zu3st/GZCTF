namespace CTFServer.Models.Request.Game;

public class GameJoinModel
{
    /// <summary>
    /// Team id
    /// </summary>
    public int TeamId { get; set; }

    /// <summary>
    /// Organization
    /// </summary>
    public string? Organization { get; set; }

    /// <summary>
    /// Invite code
    /// </summary>
    public string? InviteCode { get; set; }
}