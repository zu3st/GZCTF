using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CTFServer.Models.Data;

public class Attachment
{
    [Key]
    [Required]
    public int Id { get; set; }

    /// <summary>
    /// Attachment type
    /// </summary>
    [Required]
    public FileType Type { get; set; } = FileType.None;

    /// <summary>
    /// Flag corresponding attachment (remote file)
    /// </summary>
    [JsonIgnore]
    public string? RemoteUrl { get; set; } = string.Empty;

    /// <summary>
    /// Local file id
    /// </summary>
    [JsonIgnore]
    public int? LocalFileId { get; set; }

    /// <summary>
    /// Flag corresponding attachment (local file)
    /// </summary>
    [JsonIgnore]
    public LocalFile? LocalFile { get; set; } = default;

    /// <summary>
    /// File default Url
    /// </summary>
    [NotMapped]
    public string? Url => UrlWithName();

    /// <summary>
    /// Get attachment size
    /// </summary>
    [NotMapped]
    public long? FileSize => LocalFile?.FileSize;

    /// <summary>
    /// Attachment access link
    /// </summary>
    public string? UrlWithName(string? filename = null) => Type switch
    {
        FileType.None => null,
        FileType.Local => LocalFile?.Url(filename),
        FileType.Remote => RemoteUrl,
        _ => throw new ArgumentException(nameof(Type))
    };
}