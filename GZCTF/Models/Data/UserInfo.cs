using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CTFServer.Models.Request.Account;
using CTFServer.Models.Request.Admin;
using MemoryPack;
using Microsoft.AspNetCore.Identity;

namespace CTFServer.Models;

[MemoryPackable]
public partial class UserInfo : IdentityUser
{
    /// <summary>
    /// User Role
    /// </summary>
    [ProtectedPersonalData]
    public Role Role { get; set; } = Role.User;

    /// <summary>
    /// Most recent IP
    /// </summary>
    [ProtectedPersonalData]
    public string IP { get; set; } = "0.0.0.0";

    /// <summary>
    /// Last signed in time
    /// </summary>
    public DateTimeOffset LastSignedInUTC { get; set; } = DateTimeOffset.FromUnixTimeSeconds(0);

    /// <summary>
    /// Last visited time
    /// </summary>
    public DateTimeOffset LastVisitedUTC { get; set; } = DateTimeOffset.FromUnixTimeSeconds(0);

    /// <summary>
    /// Register time
    /// </summary>
    public DateTimeOffset RegisterTimeUTC { get; set; } = DateTimeOffset.FromUnixTimeSeconds(0);

    /// <summary>
    /// Bio
    /// </summary>
    [MaxLength(63)]
    public string Bio { get; set; } = string.Empty;

    /// <summary>
    /// Real name
    /// </summary>
    [MaxLength(7)]
    [ProtectedPersonalData]
    public string RealName { get; set; } = string.Empty;

    /// <summary>
    /// Matriculation number
    /// </summary>
    [MaxLength(31)]
    [ProtectedPersonalData]
    public string StdNumber { get; set; } = string.Empty;

    #region Db Relationship

    /// <summary>
    /// Avatar hash
    /// </summary>
    [MaxLength(64)]
    public string? AvatarHash { get; set; }

    /// <summary>
    /// List of submissions
    /// </summary>
    [MemoryPackIgnore]
    public List<Submission> Submissions { get; set; } = new();

    /// <summary>
    /// Teams participated in
    /// </summary>
    [MemoryPackIgnore]
    public List<Team> Teams { get; set; } = new();

    #endregion Db Relationship

    /// <summary>
    ///  Update user's last visited time and IP by http request
    /// </summary>
    /// <param name="context"></param>
    public void UpdateByHttpContext(HttpContext context)
    {
        LastVisitedUTC = DateTimeOffset.UtcNow;

        var remoteAddress = context.Connection.RemoteIpAddress;

        if (remoteAddress is null)
            return;

        IP = remoteAddress.ToString();
    }

    [NotMapped]
    [MemoryPackIgnore]
    public string? AvatarUrl => AvatarHash is null ? null : $"/assets/{AvatarHash}/avatar";

    internal void UpdateUserInfo(UpdateUserInfoModel model)
    {
        UserName = model.UserName ?? UserName;
        Email = model.Email ?? Email;
        Bio = model.Bio ?? Bio;
        Role = model.Role ?? Role;
        StdNumber = model.StdNumber ?? StdNumber;
        RealName = model.RealName ?? RealName;
        PhoneNumber = model.Phone ?? PhoneNumber;
        EmailConfirmed = model.EmailConfirmed ?? EmailConfirmed;
    }

    internal void UpdateUserInfo(ProfileUpdateModel model)
    {
        UserName = model.UserName ?? UserName;
        Bio = model.Bio ?? Bio;
        PhoneNumber = model.Phone ?? PhoneNumber;
        RealName = model.RealName ?? RealName;
        StdNumber = model.StdNumber ?? StdNumber;
    }
}
