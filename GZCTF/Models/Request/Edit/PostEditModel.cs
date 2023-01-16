using System.ComponentModel.DataAnnotations;

namespace CTFServer.Models.Request.Edit;

/// <summary>
/// Post edit (Edit)
/// </summary>
public class PostEditModel
{
    /// <summary>
    /// Post title
    /// </summary>
    [Required(ErrorMessage = "Title cannot be empty")]
    [MaxLength(50, ErrorMessage = "Title too long")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Post summary
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Post content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Post tags
    /// </summary>
    public string[] Tags { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Whether the post is pinned
    /// </summary>
    public bool IsPinned { get; set; } = false;
}