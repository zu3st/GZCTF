using System.ComponentModel.DataAnnotations;

namespace CTFServer.Models.Request.Account;

/// <summary>
/// Email change
/// </summary>
public class MailChangeModel
{
    /// <summary>
    /// New email
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string NewMail { get; set; } = string.Empty;
}