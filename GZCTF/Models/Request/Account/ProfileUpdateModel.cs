using System.ComponentModel.DataAnnotations;

namespace CTFServer.Models.Request.Account;

/// <summary>
/// Profile update
/// </summary>
public class ProfileUpdateModel
{
    /// <summary>
    /// Username
    /// </summary>
    [MinLength(3, ErrorMessage = "Username too short")]
    [MaxLength(15, ErrorMessage = "Username too long")]
    public string? UserName { get; set; }

    /// <summary>
    /// Bio
    /// </summary>
    [MaxLength(55, ErrorMessage = "Bio too long")]
    public string? Bio { get; set; }

    /// <summary>
    /// Phone
    /// </summary>
    [Phone(ErrorMessage = "Bad phone number")]
    public string? Phone { get; set; }

    /// <summary>
    /// Real name
    /// </summary>
    [MaxLength(7, ErrorMessage = "Real name too long")]
    public string? RealName { get; set; }

    /// <summary>
    /// Matriculation number
    /// </summary>
    [MaxLength(15, ErrorMessage = "Matriculation number too long")]
    public string? StdNumber { get; set; }
}