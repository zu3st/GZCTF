using System.ComponentModel.DataAnnotations;

namespace CTFServer.Models.Request.Edit;

/// <summary>
/// Flag submit
/// </summary>
public class FlagSubmitModel
{
    /// <summary>
    /// Flag text
    /// fix: Prevent the unexpected conversion (number/float/null) on submission by the front end
    /// </summary>
    [Required(ErrorMessage = "Flag cannot be empty")]
    [MaxLength(126, ErrorMessage = "Flag too long")]
    public string Flag { get; set; } = string.Empty;
}