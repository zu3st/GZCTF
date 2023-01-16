using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MemoryPack;

namespace CTFServer.Models;

/// <summary>
/// Game notice, will be sent to the client.
/// First blood, second blood, third blood, hint, challenge open, etc.
/// </summary>
[MemoryPackable]
public partial class GameNotice
{
    [Key]
    [Required]
    public int Id { get; set; }

    /// <summary>
    /// Notice type
    /// </summary>
    [Required]
    public NoticeType Type { get; set; } = NoticeType.Normal;

    /// <summary>
    /// Notice content
    /// </summary>
    [Required]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Publish time (UTC)
    /// </summary>
    [Required]
    [JsonPropertyName("time")]
    public DateTimeOffset PublishTimeUTC { get; set; } = DateTimeOffset.UtcNow;

    [JsonIgnore]
    [MemoryPackIgnore]
    public int GameId { get; set; }

    [JsonIgnore]
    [MemoryPackIgnore]
    public Game? Game { get; set; }

    internal static GameNotice FromSubmission(Submission submission, SubmissionType type)
        => new()
        {
            Type = type switch
            {
                SubmissionType.FirstBlood => NoticeType.FirstBlood,
                SubmissionType.SecondBlood => NoticeType.SecondBlood,
                SubmissionType.ThirdBlood => NoticeType.ThirdBlood,
                _ => NoticeType.Normal
            },
            GameId = submission.GameId,
            Content = $"Congratulations to {submission.Team.Name} for getting the {type.ToBloodString()} of ⌈{submission.Challenge.Title}⌋"
        };
}