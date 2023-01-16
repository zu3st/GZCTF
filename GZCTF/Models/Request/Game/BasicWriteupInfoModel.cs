using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CTFServer.Models.Request.Info;

namespace CTFServer.Models.Request.Game;

/// <summary>
/// Game writeup information
/// </summary>
public class BasicWriteupInfoModel
{
    /// <summary>
    /// Whether the writeup has been submitted
    /// </summary>
    public bool Submitted { get; set; } = false;

    /// <summary>
    /// Writeup file name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// File size
    /// </summary>
    public long FileSize { get; set; } = 0;

    /// <summary>
    /// Additional writeup notes
    /// </summary>
    [JsonPropertyName("note")]
    public string WriteupNote { get; set; } = string.Empty;

    internal static BasicWriteupInfoModel FromParticipation(Participation part)
        => new()
        {
            Submitted = part.Writeup is not null,
            Name = part.Writeup?.Name ?? "#",
            FileSize = part.Writeup?.FileSize ?? 0,
            WriteupNote = part.Game.WriteupNote
        };
}
