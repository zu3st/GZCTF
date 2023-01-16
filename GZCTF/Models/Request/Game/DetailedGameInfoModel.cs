using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CTFServer.Models.Request.Game;

/// <summary>
/// Detailed game information, including detailed introduction and current team registration status
/// </summary>
public class DetailedGameInfoModel
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Game title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Game summary
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Game details
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Whether the game is hidden
    /// </summary>
    public bool Hidden { get; set; } = false;

    /// <summary>
    /// List of participating organizations
    /// </summary>
    public HashSet<string>? Organizations { get; set; }

    /// <summary>
    /// Whether an invite code is required
    /// </summary>
    public bool InviteCodeRequired { get; set; } = false;

    /// <summary>
    /// Game poster URL
    /// </summary>
    [JsonPropertyName("poster")]
    public string? PosterUrl { get; set; } = string.Empty;

    /// <summary>
    /// Limit of team member count, 0 for no limit
    /// </summary>
    [JsonPropertyName("limit")]
    public int TeamMemberCountLimit { get; set; } = 0;

    /// <summary>
    /// Number of teams currently registered
    /// </summary>
    public int TeamCount { get; set; } = 0;

    /// <summary>
    /// Current organization
    /// </summary>
    public string? Organization { get; set; }

    /// <summary>
    /// Team name
    /// </summary>
    public string? TeamName { get; set; }

    /// <summary>
    /// Whether the game is in practice mode (accessible after the game ends)
    /// </summary>
    public bool PracticeMode { get; set; } = true;

    /// <summary>
    /// Team participation status
    /// </summary>
    [JsonPropertyName("status")]
    public ParticipationStatus Status { get; set; } = ParticipationStatus.Unsubmitted;

    /// <summary>
    /// Start time (UTC)
    /// </summary>
    [JsonPropertyName("start")]
    public DateTimeOffset StartTimeUTC { get; set; } = DateTimeOffset.FromUnixTimeSeconds(0);

    /// <summary>
    /// End time (UTC)
    /// </summary>
    [JsonPropertyName("end")]
    public DateTimeOffset EndTimeUTC { get; set; } = DateTimeOffset.FromUnixTimeSeconds(0);

    public DetailedGameInfoModel WithParticipation(Participation? part)
    {
        Status = part?.Status ?? ParticipationStatus.Unsubmitted;
        TeamName = part?.Team.Name;
        Organization = part?.Organization;
        return this;
    }

    internal static DetailedGameInfoModel FromGame(Models.Game game, int count)
        => new()
        {
            Id = game.Id,
            Title = game.Title,
            Hidden = game.Hidden,
            Summary = game.Summary,
            Content = game.Content,
            PracticeMode = game.PracticeMode,
            Organizations = game.Organizations,
            InviteCodeRequired = !string.IsNullOrWhiteSpace(game.InviteCode),
            TeamCount = count,
            PosterUrl = game.PosterUrl,
            StartTimeUTC = game.StartTimeUTC,
            EndTimeUTC = game.EndTimeUTC,
            TeamMemberCountLimit = game.TeamMemberCountLimit
        };
}