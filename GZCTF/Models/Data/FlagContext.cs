using System.ComponentModel.DataAnnotations;
using CTFServer.Models.Data;

namespace CTFServer.Models;

public class FlagContext
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Flag text
    /// </summary>
    [Required]
    public string Flag { get; set; } = string.Empty;

    /// <summary>
    /// Whether the flag is occupied
    /// </summary>
    public bool IsOccupied { get; set; } = false;

    #region Db Relationship

    /// <summary>
    /// Attachment id
    /// </summary>
    public int? AttachmentId { get; set; }

    /// <summary>
    /// Attachment
    /// </summary>
    public Attachment? Attachment { get; set; }

    /// <summary>
    /// Challenge id that this flag belongs to
    /// </summary>
    public int ChallengeId { get; set; }

    /// <summary>
    /// Challenge that this flag belongs to
    /// </summary>
    public Challenge? Challenge { get; set; }

    #endregion Db Relationship
}