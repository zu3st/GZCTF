using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CTFServer.Models.Request.Game;

public class GameDetailModel
{
    /// <summary>
    /// Game information
    /// </summary>
    public Dictionary<ChallengeTag, IEnumerable<ChallengeInfo>> Challenges { get; set; } = new();

    /// <summary>
    /// Scoreboard item
    /// </summary>
    [JsonPropertyName("rank")]
    public ScoreboardItem? ScoreboardItem { get; set; }

    /// <summary>
    /// Team token
    /// </summary>
    [Required]
    public string TeamToken { get; set; } = string.Empty;

    /// <summary>
    /// Writeup submission deadline
    /// </summary>
    [Required]
    [JsonPropertyName("wpddl")]
    public DateTimeOffset WriteupDeadline { get; set; } = DateTimeOffset.FromUnixTimeSeconds(0);
}
