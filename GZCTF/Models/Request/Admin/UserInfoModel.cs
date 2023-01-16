namespace CTFServer.Models.Request.Admin;

/// <summary>
/// User information (Admin)
/// </summary>
public class UserInfoModel
{
    /// <summary>
    /// User ID
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Username
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Real name
    /// </summary>
    public string? RealName { get; set; }

    /// <summary>
    /// Matriculation number
    /// </summary>
    public string? StdNumber { get; set; }

    /// <summary>
    /// Phone
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Bio
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    /// Register time
    /// </summary>
    public DateTimeOffset RegisterTimeUTC { get; set; }

    /// <summary>
    /// Last visited time
    /// </summary>
    public DateTimeOffset LastVisitedUTC { get; set; }

    /// <summary>
    /// Most recent IP
    /// </summary>
    public string IP { get; set; } = "0.0.0.0";

    /// <summary>
    /// Email
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Avatar
    /// </summary>
    public string? Avatar { get; set; }

    /// <summary>
    /// User role
    /// </summary>
    public Role? Role { get; set; }

    /// <summary>
    /// Whether the user has passed email verification (can log in)
    /// </summary>
    public bool? EmailConfirmed { get; set; }

    internal static UserInfoModel FromUserInfo(UserInfo user)
        => new()
        {
            Id = user.Id,
            IP = user.IP,
            Bio = user.Bio,
            Role = user.Role,
            Email = user.Email,
            Phone = user.PhoneNumber,
            Avatar = user.AvatarUrl,
            RealName = user.RealName,
            UserName = user.UserName,
            StdNumber = user.StdNumber,
            LastVisitedUTC = user.LastVisitedUTC,
            RegisterTimeUTC = user.RegisterTimeUTC,
            EmailConfirmed = user.EmailConfirmed
        };
}