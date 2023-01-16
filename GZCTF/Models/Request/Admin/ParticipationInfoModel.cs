namespace CTFServer.Models.Request.Admin;

/// <summary>
/// Participation object for review (Admin)
/// </summary>
public class ParticipationInfoModel
{
    /// <summary>
    /// Participation object Id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Participating team details
    /// </summary>
    public TeamWithDetailedUserInfo Team { get; set; } = default!;

    /// <summary>
    /// Registered members
    /// </summary>
    public string[] RegisteredMembers { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Participating organization
    /// </summary>
    public string? Organization { get; set; }

    /// <summary>
    /// Participation status
    /// </summary>
    public ParticipationStatus Status { get; set; } = ParticipationStatus.Pending;

    internal static ParticipationInfoModel FromParticipation(Participation part)
        => new()
        {
            Id = part.Id,
            Status = part.Status,
            Organization = part.Organization,
            RegisteredMembers = part.Members.Select(m => m.UserId).ToArray(),
            Team = TeamWithDetailedUserInfo.FromTeam(part.Team)
        };
}