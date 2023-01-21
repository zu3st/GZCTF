using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CTFServer.Models;

public class Container
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Image name
    /// </summary>
    [Required]
    public string Image { get; set; } = string.Empty;

    /// <summary>
    /// Container id
    /// </summary>
    [Required]
    public string ContainerId { get; set; } = string.Empty;

    /// <summary>
    /// Container state
    /// </summary>
    [Required]
    public ContainerStatus Status { get; set; } = ContainerStatus.Pending;

    /// <summary>
    /// Container start time
    /// </summary>
    [Required]
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Expected container stop time
    /// </summary>
    [Required]
    public DateTimeOffset ExpectStopAt { get; set; } = DateTimeOffset.UtcNow + TimeSpan.FromHours(2);

    /// <summary>
    /// Whether this container is proxied
    /// </summary>
    [Required]
    public bool IsProxy { get; set; } = false;

    /// <summary>
    /// Local ip
    /// </summary>
    [Required]
    public string IP { get; set; } = string.Empty;

    /// <summary>
    /// Local port
    /// </summary>
    [Required]
    public int Port { get; set; }

    /// <summary>
    /// Public ip
    /// </summary>
    public string? PublicIP { get; set; }

    /// <summary>
    /// Public port
    /// </summary>
    public int? PublicPort { get; set; }

    /// <summary>
    /// Container instance access method
    /// </summary>
    [NotMapped]
    public string Entry => InstanceEntry();

    /// <summary>
    /// Attachment access link
    /// </summary>

    /// <summary>
    /// Entry presentation, supporting custom presentation for ssh and http
    /// </summary>
    public string InstanceEntry() => Port switch
    {
        // Inside the container, derive the ContainerId from hostname
        22 => $"ssh u{ContainerId.Split('-').Last()}@{PublicIP ?? IP} -p {PublicPort ?? Port}",
        80 when PublicIP != null => $"https://{ContainerId}.{PublicIP}",
        _ => $"{PublicIP ?? IP}:{PublicPort ?? Port}"
    };

    #region Db Relationship

    /// <summary>
    /// Instance of the game challenge
    /// </summary>
    public Instance? Instance { get; set; }

    /// <summary>
    /// Instance object ID
    /// </summary>
    public int InstanceId { get; set; }

    #endregion Db Relationship
}