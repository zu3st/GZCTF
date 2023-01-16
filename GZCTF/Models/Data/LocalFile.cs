using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace CTFServer.Models;

[Index(nameof(Hash))]
public class LocalFile
{
    [Key]
    [JsonIgnore]
    public int Id { get; set; }

    /// <summary>
    /// File hash
    /// </summary>
    [MaxLength(64)]
    public string Hash { get; set; } = string.Empty;

    /// <summary>
    /// Upload time (UTC)
    /// </summary>
    [JsonIgnore]
    public DateTimeOffset UploadTimeUTC { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// File size
    /// </summary>
    [JsonIgnore]
    public long FileSize { get; set; } = 0;

    /// <summary>
    /// File name
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Reference count
    /// </summary>
    [JsonIgnore]
    public uint ReferenceCount { get; set; } = 1;

    /// <summary>
    /// Gets file storage location
    /// </summary>
    [NotMapped]
    [JsonIgnore]
    public string Location => $"{Hash[..2]}/{Hash[2..4]}";

    /// <summary>
    /// Gets file Url
    /// </summary>
    public string Url(string? filename = null) => $"/assets/{Hash}/{filename ?? Name}";
}