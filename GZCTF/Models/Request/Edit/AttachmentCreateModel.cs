namespace CTFServer.Models.Request.Edit;

/// <summary>
/// Create attachment information (Edit)
/// </summary>
public class AttachmentCreateModel
{
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