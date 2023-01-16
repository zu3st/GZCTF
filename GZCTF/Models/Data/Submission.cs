using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace CTFServer.Models;

[Index(nameof(UserId))]
public class Submission
{
    [Key]
    [JsonIgnore]
    public int Id { get; set; }

    /// <summary>
    /// Answer text
    /// </summary>
    [MaxLength(127)]
    public string Answer { get; set; } = string.Empty;

    /// <summary>
    /// Submission status
    /// </summary>
    public AnswerResult Status { get; set; } = AnswerResult.Accepted;

    /// <summary>
    /// Submit time (UTC)
    /// </summary>
    [JsonPropertyName("time")]
    public DateTimeOffset SubmitTimeUTC { get; set; } = DateTimeOffset.FromUnixTimeSeconds(0);

    /// <summary>
    /// Submitter name
    /// </summary>
    [JsonPropertyName("user")]
    public string UserName => User?.UserName ?? string.Empty;

    /// <summary>
    /// Submitter team name
    /// </summary>
    [JsonPropertyName("team")]
    public string TeamName => Team?.Name ?? string.Empty;

    /// <summary>
    /// Challenge name
    /// </summary>
    [JsonPropertyName("challenge")]
    public string ChallengeName => Challenge?.Title ?? string.Empty;

    #region Db Relationship

    /// <summary>
    /// Submitter user id
    /// </summary>
    [JsonIgnore]
    public string? UserId { get; set; }

    /// <summary>
    /// Submitter user object
    /// </summary>
    [JsonIgnore]
    public UserInfo User { get; set; } = default!;

    /// <summary>
    /// Submitter team id
    /// </summary>
    [JsonIgnore]
    public int TeamId { get; set; }

    /// <summary>
    /// Submitter team object
    /// </summary>
    [JsonIgnore]
    public Team Team { get; set; } = default!;

    /// <summary>
    /// Participation id
    /// </summary>
    [JsonIgnore]
    public int ParticipationId { get; set; }

    /// <summary>
    /// Participation object
    /// </summary>
    [JsonIgnore]
    public Participation Participation { get; set; } = default!;

    /// <summary>
    /// Game id
    /// </summary>
    [JsonIgnore]
    public int GameId { get; set; }

    /// <summary>
    /// Game object
    /// </summary>
    [JsonIgnore]
    public Game Game { get; set; } = default!;

    /// <summary>
    /// Challenge id
    /// </summary>
    [JsonIgnore]
    public int ChallengeId { get; set; }

    /// <summary>
    /// Challenge object
    /// </summary>
    [JsonIgnore]
    public Challenge Challenge { get; set; } = default!;

    #endregion Db Relationship
}