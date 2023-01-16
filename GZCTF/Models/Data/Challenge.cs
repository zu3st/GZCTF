using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using CTFServer.Models.Data;
using CTFServer.Models.Request.Edit;
using CTFServer.Utils;

namespace CTFServer.Models;

public class Challenge
{
    [Key]
    [Required]
    public int Id { get; set; }

    /// <summary>
    /// Challenge title
    /// </summary>
    [Required(ErrorMessage = "Title cannot be empty")]
    [MinLength(1, ErrorMessage = "Title too short")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Challenge content
    /// </summary>
    [Required(ErrorMessage = "Body cannot be empty")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Whether the challenge is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = false;

    /// <summary>
    /// Challenge tag
    /// </summary>
    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ChallengeTag Tag { get; set; } = ChallengeTag.Misc;

    /// <summary>
    /// Challenge type, cannot be changed after creation
    /// </summary>
    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ChallengeType Type { get; set; } = ChallengeType.StaticAttachment;

    /// <summary>
    /// Challenge hints
    /// </summary>
    public List<string>? Hints { get; set; }

    /// <summary>
    /// Flag template, used to generate Flag based on Token and Challenge and Game information
    /// </summary>
    public string? FlagTemplate { get; set; }

    /// <summary>
    /// Container image
    /// </summary>
    public string? ContainerImage { get; set; } = string.Empty;

    /// <summary>
    /// Memory limit (MB)
    /// </summary>
    public int? MemoryLimit { get; set; } = 64;

    /// <summary>
    /// Storage limit (MB)
    /// </summary>
    public int? StorageLimit { get; set; } = 256;

    /// <summary>
    /// CPU count limit
    /// </summary>
    public int? CPUCount { get; set; } = 1;

    /// <summary>
    /// Exposed container port
    /// </summary>
    public int? ContainerExposePort { get; set; } = 80;

    /// <summary>
    /// Whether the container is privileged
    /// </summary>
    public bool? PrivilegedContainer { get; set; } = false;

    /// <summary>
    /// Number of times the challenge has been solved
    /// </summary>
    [Required]
    public int AcceptedCount { get; set; } = 0;

    /// <summary>
    /// Number of answers submitted
    /// </summary>
    [Required]
    [JsonIgnore]
    public int SubmissionCount { get; set; } = 0;

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
    public double Difficulty { get; set; } = 5;

    /// <summary>
    /// Current score
    /// </summary>
    [NotMapped]
    public int CurrentScore =>
        AcceptedCount <= 1 ? OriginalScore : (int)Math.Floor(
        OriginalScore * (MinScoreRate +
            (1.0 - MinScoreRate) * Math.Exp((1 - AcceptedCount) / Difficulty)
        ));

    /// <summary>
    /// Unified file name (only for dynamic attachment)
    /// </summary>
    public string? FileName { get; set; } = "attachment";

    #region Db Relationship

    /// <summary>
    /// Attachment Id
    /// </summary>
    public int? AttachmentId { get; set; }

    /// <summary>
    /// Challenge attachment (dynamic attachment is stored in FlagInfoModel)
    /// </summary>
    public Attachment? Attachment { get; set; }

    /// <summary>
    /// Test container Id
    /// </summary>
    public string? TestContainerId { get; set; }

    /// <summary>
    /// Test container
    /// </summary>
    public Container? TestContainer { get; set; }

    /// <summary>
    /// List of accepted flags
    /// </summary>
    public List<FlagContext> Flags { get; set; } = new();

    /// <summary>
    /// List of submissions
    /// </summary>
    public List<Submission> Submissions { get; set; } = new();

    /// <summary>
    /// List of instances
    /// </summary>
    public List<Instance> Instances { get; set; } = new();

    /// <summary>
    /// Teams that participated in this challenge
    /// </summary>
    public HashSet<Participation> Teams { get; set; } = new();

    /// <summary>
    /// Game id this challenge belongs to
    /// </summary>
    public int GameId { get; set; }

    /// <summary>
    /// Game this challenge belongs to
    /// </summary>
    public Game Game { get; set; } = default!;

    #endregion Db Relationship

    internal string GenerateFlag(Participation part)
    {
        if (string.IsNullOrEmpty(FlagTemplate))
            return $"flag{Guid.NewGuid():B}";

        if (FlagTemplate.Contains("[TEAM_HASH]"))
        {
            var flag = FlagTemplate;
            if (FlagTemplate.StartsWith("[LEET]"))
                flag = Codec.Leet.LeetFlag(FlagTemplate[6..]);

            var hash = Codec.StrSHA256($"{part.Token}::{part.Game.PrivateKey}::{Id}");
            return flag.Replace("[TEAM_HASH]", hash[12..24]);
        }

        return Codec.Leet.LeetFlag(FlagTemplate);
    }

    internal string GenerateTestFlag()
    {
        if (string.IsNullOrEmpty(FlagTemplate))
            return "flag{GZCTF_dynamic_flag_test}";

        if (FlagTemplate.StartsWith("[LEET]"))
            return Codec.Leet.LeetFlag(FlagTemplate[6..]);

        return Codec.Leet.LeetFlag(FlagTemplate);
    }

    internal Challenge Update(ChallengeUpdateModel model)
    {
        Title = model.Title ?? Title;
        Content = model.Content ?? Content;
        Tag = model.Tag ?? Tag;
        Hints = model.Hints ?? Hints;
        IsEnabled = model.IsEnabled ?? IsEnabled;
        // Only set FlagTemplate to null when it pass an empty string (but not null)
        FlagTemplate = model.FlagTemplate is null ? FlagTemplate :
            string.IsNullOrWhiteSpace(model.FlagTemplate) ? null : model.FlagTemplate;
        CPUCount = model.CPUCount ?? CPUCount;
        MemoryLimit = model.MemoryLimit ?? MemoryLimit;
        StorageLimit = model.StorageLimit ?? StorageLimit;
        ContainerImage = model.ContainerImage ?? ContainerImage;
        PrivilegedContainer = model.PrivilegedContainer ?? PrivilegedContainer;
        ContainerExposePort = model.ContainerExposePort ?? ContainerExposePort;
        OriginalScore = model.OriginalScore ?? OriginalScore;
        MinScoreRate = model.MinScoreRate ?? MinScoreRate;
        Difficulty = model.Difficulty ?? Difficulty;
        FileName = model.FileName ?? FileName;

        return this;
    }
}
