using System.ComponentModel.DataAnnotations;

namespace CTFServer.Models.Request.Admin;

/// <summary>
/// User information update (Admin)
/// </summary>
public class UpdateUserInfoModel
{
    /// <summary>
    /// Username
    /// </summary>
    [MinLength(3, ErrorMessage = "Username too short")]
    [MaxLength(15, ErrorMessage = "Username too long")]
    public string? UserName { get; set; }

    /// <summary>
    /// Email
    /// </summary>
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string? Email { get; set; }

    /// <summary>
    /// Bio
    /// </summary>
    [MaxLength(50, ErrorMessage = "Bio too long")]
    public string? Bio { get; set; }

    /// <summary>
    /// Phone
    /// </summary>
    [Phone(ErrorMessage = "Phone number invalid")]
    public string? Phone { get; set; }

    /// <summary>
    /// Real name
    /// </summary>
    [MaxLength(6, ErrorMessage = "Real name too long")]
    public string? RealName { get; set; }

    /// <summary>
    /// Matriculation number
    /// </summary>
    [MaxLength(10, ErrorMessage = "Matriculation number too long")]
    public string? StdNumber { get; set; }

    /// <summary>
    /// Whether the user has passed email verification (can log in)
    /// </summary>
    public bool? EmailConfirmed { get; set; }

    /// <summary>
    /// User role
    /// </summary>
    public Role? Role { get; set; }
}