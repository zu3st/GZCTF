using System.ComponentModel.DataAnnotations;
using CTFServer.Models.Data;
using CTFServer.Models.Request.Game;

namespace CTFServer.Models.Request.Edit;

/// <summary>
/// Challenge detail information (Edit)
/// </summary>
public class ChallengeEditDetailModel
{
    /// <summary>
    /// Challenge Id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Challenge title
    /// </summary>
    [Required]
    [MinLength(1, ErrorMessage = "Title too short")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Challenge content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Challenge tag
    /// </summary>
    [Required]
    public ChallengeTag Tag { get; set; } = ChallengeTag.Misc;

    /// <summary>
    /// Challenge type
    /// </summary>
    [Required]
    public ChallengeType Type { get; set; } = ChallengeType.StaticAttachment;

    /// <summary>
    /// Challenge hints 
    /// </summary>
    public string[] Hints { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Flag template, used to generate Flag based on Token and Challenge and Game information
    /// </summary>
    public string? FlagTemplate { get; set; }

    /// <summary>
    /// Whether the challenge is enabled
    /// </summary>
    [Required]
    public bool IsEnabled { get; set; }

    #region Container

    /// <summary>
    /// Container image
    /// </summary>
    [Required]
    public string? ContainerImage { get; set; } = string.Empty;

    /// <summary>
    /// Memory limit (MB)
    /// </summary>
    [Required]
    public int? MemoryLimit { get; set; } = 64;

    /// <summary>
    /// CPU count limit
    /// </summary>
    [Required]
    public int? CPUCount { get; set; } = 1;

    /// <summary>
    /// Storage limit (MB)
    /// </summary>
    [Required]
    public int? StorageLimit { get; set; } = 256;

    /// <summary>
    /// Exposed container port
    /// </summary>
    [Required]
    public int? ContainerExposePort { get; set; } = 80;

    /// <summary>
    /// Whether the container is privileged
    /// </summary>
    public bool? PrivilegedContainer { get; set; } = false;

    #endregion Container

    #region Score

    /// <summary>
    /// Initial score
    /// </summary>
    [Required]
    public int OriginalScore { get; set; } = 500;

    /// <summary>
    /// Minimum score ratio
    /// </summary>
    [Required]
    [Range(0, 1)]
    public double MinScoreRate { get; set; } = 0.25;

    /// <summary>
    /// Difficulty factor
    /// </summary>
    [Required]
    public double Difficulty { get; set; } = 3;

    #endregion Score

    /// <summary>
    /// Number of solves
    /// </summary>
    [Required]
    public int AcceptedCount { get; set; } = 0;

    /// <summary>
    /// Unified file name (only for dynamic attachment)
    /// </summary>
    public string? FileName { get; set; } = string.Empty;

    /// <summary>
    /// Challenge attachment (dynamic attachment is stored in FlagInfoModel)
    /// </summary>
    public Attachment? Attachment { get; set; }

    /// <summary>
    /// Test container information
    /// </summary>
    public ContainerInfoModel? TestContainer { get; set; }

    /// <summary>
    /// Challenge Flag Information
    /// </summary>
    [Required]
    public FlagInfoModel[] Flags { get; set; } = Array.Empty<FlagInfoModel>();

    internal static ChallengeEditDetailModel FromChallenge(Challenge chal)
        => new()
        {
            Id = chal.Id,
            Title = chal.Title,
            Content = chal.Content,
            Tag = chal.Tag,
            Type = chal.Type,
            FlagTemplate = chal.FlagTemplate,
            Hints = chal.Hints?.ToArray() ?? Array.Empty<string>(),
            IsEnabled = chal.IsEnabled,
            ContainerImage = chal.ContainerImage,
            MemoryLimit = chal.MemoryLimit,
            CPUCount = chal.CPUCount,
            StorageLimit = chal.StorageLimit,
            ContainerExposePort = chal.ContainerExposePort,
            PrivilegedContainer = chal.PrivilegedContainer,
            OriginalScore = chal.OriginalScore,
            MinScoreRate = chal.MinScoreRate,
            Difficulty = chal.Difficulty,
            FileName = chal.FileName,
            AcceptedCount = chal.AcceptedCount,
            Attachment = chal.Attachment,
            TestContainer = chal.TestContainer is null ? null :
                ContainerInfoModel.FromContainer(chal.TestContainer),
            Flags = (from flag in chal.Flags
                     select FlagInfoModel.FromFlagContext(flag))
                    .ToArray()
        };
}
