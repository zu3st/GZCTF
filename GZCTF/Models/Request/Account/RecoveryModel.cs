using System.ComponentModel.DataAnnotations;

namespace CTFServer.Models.Request.Account;

/// <summary>
/// Account recovery
/// </summary>
public class RecoveryModel
{
    /// <summary>
    /// User email
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string? Email { get; set; }

    /// <summary>
    /// Google Recaptcha Token
    /// </summary>
    public string? GToken { get; set; }
}