using System.ComponentModel.DataAnnotations;

namespace CTFServer.Models.Request.Edit;

/// <summary>
/// Game notice (Edit)
/// </summary>
public class GameNoticeModel
{
    /// <summary>
    /// Notice content
    /// </summary>
    [Required(ErrorMessage = "Content cannot be empty")]
    public string Content { get; set; } = string.Empty;
}