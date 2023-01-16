using System.ComponentModel.DataAnnotations;

namespace CTFServer.Models.Request.Account;

/// <summary>
/// Password reset
/// </summary>
public class PasswordResetModel
{
    /// <summary>
    /// Password
    /// </summary>
    [MinLength(6, ErrorMessage = "Password too short")]
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Email
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Token in Base64 format received in the email
    /// </summary>
    [Required(ErrorMessage = "Token is required")]
    public string? RToken { get; set; }
}