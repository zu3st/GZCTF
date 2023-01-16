using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CTFServer.Utils;

namespace CTFServer.Models.Request.Edit;

/// <summary>
/// Game information (Edit)
/// </summary>
public class GameInfoModel
{
    /// <summary>
    /// Game id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Game title
    /// </summary>
    [Required]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Whether the game is hidden
    /// </summary>
    public bool Hidden { get; set; } = false;

    /// <summary>
    /// Game summary
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Game details
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Whether to enroll teams without manual review
    /// </summary>
    public bool AcceptWithoutReview { get; set; } = false;

    /// <summary>
    /// Game invite code
    /// </summary>
    [MaxLength(32, ErrorMessage = "Invite code too long")]
    public string? InviteCode { get; set; }

    /// <summary>
    /// List of participating organizations
    /// </summary>
    public HashSet<string>? Organizations { get; set; }

    /// <summary>
    /// Limit of team member count, 0 for no limit
    /// </summary>
    public int TeamMemberCountLimit { get; set; } = 0;

    /// <summary>
    /// Limit of concurrent containers per team, 0 for no limit
    /// </summary>
    public int ContainerCountLimit { get; set; } = 3;

    /// <summary>
    /// Game poster URL
    /// </summary>
    [JsonPropertyName("poster")]
    public string? PosterUrl { get; set; } = string.Empty;

    /// <summary>
    /// Game signature public key
    /// </summary>
    public string PublicKey { get; set; } = string.Empty;

    /// <summary>
    /// Whether the game is in practice mode (accessible after the game ends)
    /// </summary>
    public bool PracticeMode { get; set; } = true;

    /// <summary>un
    /// Start time (UTC)
    /// </summary>
    [Required]
    [JsonPropertyName("start")]
    public DateTimeOffset StartTimeUTC { get; set; } = DateTimeOffset.FromUnixTimeSeconds(0);

    /// <summary>
    /// End time (UTC)
    /// </summary>
    [Required]
    [JsonPropertyName("end")]
    public DateTimeOffset EndTimeUTC { get; set; } = DateTimeOffset.FromUnixTimeSeconds(0);

    /// <summary>
    /// Writeup submission deadline
    /// </summary>
    [Required]
    [JsonPropertyName("wpddl")]
    public DateTimeOffset WriteupDeadline { get; set; } = DateTimeOffset.FromUnixTimeSeconds(0);

    /// <summary>
    /// Writeup additional notes
    /// </summary>
    [JsonPropertyName("wpNote")]
    public string WriteupNote { get; set; } = string.Empty;

    /// <summary>
    /// First three bloods bonus amount
    /// </summary>
    [JsonPropertyName("bloodBonus")]
    public long BloodBonusValue { get; set; } = BloodBonus.DefaultValue;

    internal static GameInfoModel FromGame(Models.Game game)
        => new()
        {
            Id = game.Id,
            Title = game.Title,
            Summary = game.Summary,
            Content = game.Content,
            Hidden = game.Hidden,
            PracticeMode = game.PracticeMode,
            PosterUrl = game.PosterUrl,
            InviteCode = game.InviteCode,
            PublicKey = game.PublicKey,
            Organizations = game.Organizations,
            AcceptWithoutReview = game.AcceptWithoutReview,
            TeamMemberCountLimit = game.TeamMemberCountLimit,
            ContainerCountLimit = game.ContainerCountLimit,
            StartTimeUTC = game.StartTimeUTC,
            EndTimeUTC = game.EndTimeUTC,
            WriteupDeadline = game.WriteupDeadline,
            WriteupNote = game.WriteupNote,
            BloodBonusValue = game.BloodBonus.Val
        };
}
