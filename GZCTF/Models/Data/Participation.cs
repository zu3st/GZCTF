using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CTFServer.Models;

public class Participation
{
    [Key]
    [JsonIgnore]
    public int Id { get; set; }

    /// <summary>
    /// Participation status
    /// </summary>
    [Required]
    public ParticipationStatus Status { get; set; } = ParticipationStatus.Pending;

    /// <summary>
    /// Team token
    /// </summary>
    [Required]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Organization
    /// </summary>
    public string? Organization { get; set; }

    /// <summary>
    /// Writeup
    /// </summary>
    public LocalFile? Writeup { get; set; }

    #region Db Relationship

    /// <summary>
    /// Members of the team
    /// </summary>
    public HashSet<UserParticipation> Members { get; set; } = new();

    /// <summary>
    /// Attended challenges
    /// </summary>
    public HashSet<Challenge> Challenges { get; set; } = new();

    /// <summary>
    /// Challenge instance of the team
    /// </summary>
    public List<Instance> Instances { get; set; } = new();

    /// <summary>
    /// Submissions of the team
    /// </summary>
    public List<Submission> Submissions { get; set; } = new();

    [Required]
    public int GameId { get; set; }

    public Game Game { get; set; } = default!;

    [Required]
    public int TeamId { get; set; }

    public Team Team { get; set; } = default!;

    #endregion Db Relationship
}