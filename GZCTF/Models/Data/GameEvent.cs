using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CTFServer.Models;

/// <summary>
/// Game event, logged but not sent to client
/// Covers Flag submission, container start/stop, cheating, challenge score change
/// </summary>
public class GameEvent
{
    [Key]
    [JsonIgnore]
    public int Id { get; set; }

    /// <summary>
    /// Event type
    /// </summary>
    [Required]
    public EventType Type { get; set; } = EventType.Normal;

    /// <summary>
    /// Event content
    /// </summary>
    [Required]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Publish time (UTC)
    /// </summary>
    [Required]
    [JsonPropertyName("time")]
    public DateTimeOffset PublishTimeUTC { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Related user name
    /// </summary>
    [JsonPropertyName("user")]
    public string UserName => User?.UserName ?? string.Empty;

    /// <summary>
    /// Related team name
    /// </summary>
    [JsonPropertyName("team")]
    public string TeamName => Team?.Name ?? string.Empty;

    [JsonIgnore]
    public string? UserId { get; set; }

    [JsonIgnore]
    public UserInfo? User { get; set; }

    [JsonIgnore]
    public int TeamId { get; set; }

    [JsonIgnore]
    public Team? Team { get; set; }

    [JsonIgnore]
    public int GameId { get; set; }

    [JsonIgnore]
    public Game? Game { get; set; }

    internal static GameEvent FromSubmission(Submission submission, SubmissionType type, AnswerResult ans)
        => new()
        {
            TeamId = submission.TeamId,
            UserId = submission.UserId,
            GameId = submission.GameId,
            Type = EventType.FlagSubmit,
            Content = $"[{ans.ToShortString()}] {submission.Answer}  {submission.Challenge.Title}#{submission.ChallengeId}"
        };
}