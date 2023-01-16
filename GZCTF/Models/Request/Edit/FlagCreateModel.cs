using System.ComponentModel.DataAnnotations;

namespace CTFServer.Models.Request.Edit;

/// <summary>
/// Create Flag information (Edit)
/// </summary>
public class FlagCreateModel
{
    /// <summary>
    /// Flag text
    /// </summary>
    [Required]
    [MaxLength(125, ErrorMessage = "Flag string too long")]
    public string Flag { get; set; } = string.Empty;

    /// <summary>
    /// Attachment type
    /// </summary>
    public FileType AttachmentType { get; set; } = FileType.None;

    /// <summary>
    /// File hash (local file)
    /// </summary>
    public string? FileHash { get; set; } = string.Empty;

    /// <summary>
    /// File URL (remote file)
    /// </summary>
    public string? RemoteUrl { get; set; } = string.Empty;
}