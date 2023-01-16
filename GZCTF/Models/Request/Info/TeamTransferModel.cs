using System.ComponentModel.DataAnnotations;

namespace CTFServer.Models.Request.Info;

public class TeamTransferModel
{
    /// <summary>
    /// New captain id
    /// </summary>
    [Required]
    public string NewCaptainId { get; set; } = string.Empty;
}
