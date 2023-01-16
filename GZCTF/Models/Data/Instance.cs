using System.ComponentModel.DataAnnotations;
using CTFServer.Models.Data;

namespace CTFServer.Models;

public class Instance
{
    /// <summary>
    /// Whether the instance is solved
    /// </summary>
    public bool IsSolved { get; set; } = false;

    /// <summary>
    /// Whether the instance is loaded   
    /// </summary>
    public bool IsLoaded { get; set; } = false;

    /// <summary>
    /// Custom score (unused)
    /// </summary>
    public int Score { get; set; } = 0;

    /// <summary>
    /// The last time a container operation was performed on this instance, used to prevent too frequent operations
    /// </summary>
    public DateTimeOffset LastContainerOperation { get; set; } = DateTimeOffset.MinValue;

    #region Db Relationship

    public int? FlagId { get; set; }

    /// <summary>
    /// Flag Context
    /// </summary>
    public FlagContext? FlagContext { get; set; } = default!;

    [Required]
    public int ChallengeId { get; set; }

    /// <summary>
    /// Challenge belonging to this instance
    /// </summary>
    public Challenge Challenge { get; set; } = default!;

    public string? ContainerId { get; set; }

    /// <summary>
    /// Container object
    /// </summary>
    public Container? Container { get; set; }

    [Required]
    public int ParticipationId { get; set; }

    /// <summary>
    /// Participation object
    /// </summary>
    public Participation Participation { get; set; } = default!;

    #endregion Db Relationship

    /// <summary>
    /// Gets instance attachment
    /// </summary>
    internal Attachment? Attachment => Challenge.Type == ChallengeType.DynamicAttachment ?
        FlagContext?.Attachment : Challenge.Attachment;

    /// <summary>
    /// Gets instance attachment link
    /// </summary>
    internal string? AttachmentUrl => Challenge.Type == ChallengeType.DynamicAttachment ?
        FlagContext?.Attachment?.UrlWithName(Challenge.FileName) :
        Challenge.Attachment?.UrlWithName();
}
