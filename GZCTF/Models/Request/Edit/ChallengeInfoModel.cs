using System.ComponentModel.DataAnnotations;

namespace CTFServer.Models.Request.Edit;

/// <summary>
/// Basic challenge information (Edit)
/// </summary>
public class ChallengeInfoModel
{
    /// <summary>
    /// Challenge id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Challenge title
    /// </summary>
    [Required(ErrorMessage = "Title is required")]
    [MinLength(1, ErrorMessage = "Title too short")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Challenge tag
    /// </summary>
    public ChallengeTag Tag { get; set; } = ChallengeTag.Misc;

    /// <summary>
    /// Challenge type
    /// </summary>
    public ChallengeType Type { get; set; } = ChallengeType.StaticAttachment;

    /// <summary>
    /// Whether the challenge is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = false;

    /// <summary>
    /// Challenge score
    /// </summary>
    public int Score { get; set; } = 500;

    /// <summary>
    /// Minimum score
    /// </summary>
    public int MinScore { get; set; } = 0;

    /// <summary>
    /// Initial score
    /// </summary>
    public int OriginalScore { get; set; } = 500;

    internal static ChallengeInfoModel FromChallenge(Challenge challenge)
        => new()
        {
            Id = challenge.Id,
            Title = challenge.Title,
            Tag = challenge.Tag,
            Type = challenge.Type,
            Score = challenge.CurrentScore,
            MinScore = (int)Math.Floor(challenge.MinScoreRate * challenge.OriginalScore),
            OriginalScore = challenge.OriginalScore,
            IsEnabled = challenge.IsEnabled
        };
}