using System.ComponentModel.DataAnnotations;

namespace CTFServer.Models.Request.Account;

/// <summary>
/// Register
/// </summary>
public class RegisterModel
{
    /// <summary>
    /// Username
    /// </summary>
    [Required(ErrorMessage = "Username is required")]
    [MinLength(3, ErrorMessage = "Username too short")]
    [MaxLength(15, ErrorMessage = "Username too long")]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Password
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Email
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Google Recaptcha Token
    /// </summary>
    public string? GToken { get; set; }
}