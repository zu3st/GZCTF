using System.ComponentModel.DataAnnotations;

namespace CTFServer.Models.Request.Account;

/// <summary>
/// Account verification
/// </summary>
public class AccountVerifyModel
{
    /// <summary>
    /// Token received in the email in Base64 format
    /// </summary>
    [Required(ErrorMessage = "Token is required")]
    public string? Token { get; set; }

    /// <summary>
    /// User email in Base64 format
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    public string? Email { get; set; }
}