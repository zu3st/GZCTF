using System.ComponentModel.DataAnnotations;

namespace CTFServer.Models.Request.Edit;

/// <summary>
/// Challenge update information (Edit)
/// </summary>
public class ChallengeUpdateModel
{
    /// <summary>
    /// Challenge title
    /// </summary>
    [MinLength(1, ErrorMessage = "Title too short")]
    public string? Title { get; set; }

    /// <summary>
    /// Challenge body
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Flag template, used to generate flag based on token, challenge, and game information
    /// </summary>
    [MaxLength(120, ErrorMessage = "Flag template too long")]
    public string? FlagTemplate { get; set; }

    /// <summary>
    /// Challenge tag
    /// </summary>
    public ChallengeTag? Tag { get; set; }

    /// <summary>
    /// Challenge hints
    /// </summary>
    public List<string>? Hints { get; set; }

    /// <summary>
    /// Whether the challenge is enabled
    /// </summary>
    public bool? IsEnabled { get; set; }

    #region Container

    /// <summary>
    /// Container image
    /// </summary>
    public string? ContainerImage { get; set; }

    /// <summary>
    /// Memory limit (MB)
    /// </summary>
    [Range(32, 1048576, ErrorMessage = "{0} must be between {1} - {2}")]
    public int? MemoryLimit { get; set; }

    /// <summary>
    /// CPU count limit
    /// </summary>
    [Range(1, 1024, ErrorMessage = "{0} must be between {1} - {2}")]
    public int? CPUCount { get; set; }

    /// <summary>
    /// Storage limit (MB)
    /// </summary>
    [Range(128, 1048576, ErrorMessage = "{0} must be between {1} - {2}")]
    public int? StorageLimit { get; set; }

    /// <summary>
    /// Exposed container port
    /// </summary>
    public int? ContainerExposePort { get; set; }

    /// <summary>
    /// Whether the container is privileged
    /// </summary>
    public bool? PrivilegedContainer { get; set; } = false;

    #endregion Container

    #region Score

    /// <summary>
    /// Initial score
    /// </summary>
    public int? OriginalScore { get; set; }

    /// <summary>
    /// Minimum score ratio
    /// </summary>
    [Range(0, 1)]
    public double? MinScoreRate { get; set; }

    /// <summary>
    /// Difficulty factor
    /// </summary>
    public double? Difficulty { get; set; }

    #endregion Score

    /// <summary>
    /// Uniform file name
    /// </summary>
    public string? FileName { get; set; }
}
