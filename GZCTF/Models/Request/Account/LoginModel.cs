using System.ComponentModel.DataAnnotations;

namespace CTFServer.Models.Request.Account;

/// <summary>
/// Login
/// </summary>
public class LoginModel
{
    /// <summary>
    /// Username or email
    /// </summary>
    [Required]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Password
    /// </summary>
    [Required]
    [MinLength(6, ErrorMessage = "Password too short")]
    public string Password { get; set; } = string.Empty;
}