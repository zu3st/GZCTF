using System.Text.Json.Serialization;
using CTFServer.Models.Request.Info;

namespace CTFServer.Models.Request.Admin;

/// <summary>
/// Game writeup information
/// </summary>
public class WriteupInfoModel
{
    /// <summary>
    /// Participation ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Team information
    /// </summary>
    public TeamInfoModel Team { get; set; } = default!;

    /// <summary>
    /// File URL
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// File upload time
    /// </summary>
    public DateTimeOffset UploadTimeUTC { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Writeup file
    /// </summary>
    [JsonIgnore]
    public LocalFile File { get; set; } = default!;

    internal static WriteupInfoModel? FromParticipation(Participation part)
        => part.Writeup is null ? null : new()
        {
            Id = part.Id,
            Team = TeamInfoModel.FromTeam(part.Team, false),
            File = part.Writeup,
            Url = part.Writeup.Url(),
            UploadTimeUTC = part.Writeup.UploadTimeUTC
        };
}