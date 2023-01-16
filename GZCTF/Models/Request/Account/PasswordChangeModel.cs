using System.ComponentModel.DataAnnotations;

namespace CTFServer.Models.Request.Account;

/// <summary>
/// Password change
/// </summary>
public class PasswordChangeModel
{
    /// <summary>
    /// Old password
    /// </summary>
    [Required(ErrorMessage = "Old password is required")]
    [MinLength(6, ErrorMessage = "Old password too short")]
    public string Old { get; set; } = string.Empty;

    /// <summary>
    /// New password
    /// </summary>
    [Required(ErrorMessage = "New password is required")]
    [MinLength(6, ErrorMessage = "New password too short")]
    public string New { get; set; } = string.Empty;
}