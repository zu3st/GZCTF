using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MemoryPack;

namespace CTFServer.Models.Request.Game;

/// <summary>
/// Basic game information, not including detailed information and team registration status
/// </summary>
[MemoryPackable]
public partial class BasicGameInfoModel
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
    /// Game poster URL
    /// </summary>
    [JsonPropertyName("poster")]
    public string? PosterUrl { get; set; } = string.Empty;

    /// <summary>
    /// Limit of team member count, 0 for no limit
    /// </summary>
    [JsonPropertyName("limit")]
    public int TeamMemberLimitCount { get; set; } = 0;

    /// <summary>
    /// Game start time (UTC)
    /// </summary>
    [JsonPropertyName("start")]
    public DateTimeOffset StartTimeUTC { get; set; } = DateTimeOffset.FromUnixTimeSeconds(0);

    /// <summary>
    /// Game end time (UTC)
    /// </summary>
    [JsonPropertyName("end")]
    public DateTimeOffset EndTimeUTC { get; set; } = DateTimeOffset.FromUnixTimeSeconds(0);

    internal static BasicGameInfoModel FromGame(Models.Game game)
        => new()
        {
            Id = game.Id,
            Title = game.Title,
            Summary = game.Summary,
            PosterUrl = game.PosterUrl,
            StartTimeUTC = game.StartTimeUTC,
            EndTimeUTC = game.EndTimeUTC,
            TeamMemberLimitCount = game.TeamMemberCountLimit
        };
}