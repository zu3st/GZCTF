using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace CTFServer.Models;

[Index(nameof(ParticipationId))]
public class UserParticipation
{
    public UserParticipation() { }

    public UserParticipation(UserInfo user, Game game, Team team)
    {
        User = user;
        Game = game;
        Team = team;
    }

    /// <summary>
    /// Particpating user id
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Participating user
    /// </summary>
    public UserInfo User { get; set; } = default!;

    /// <summary>
    /// Participating team id
    /// </summary>
    [Required]
    public int TeamId { get; set; }

    /// <summary>
    /// Participating team
    /// </summary>
    public Team Team { get; set; } = default!;

    /// <summary>
    /// Game Id
    /// </summary>
    [Required]
    public int GameId { get; set; }

    /// <summary>
    /// Game
    /// </summary>
    public Game Game { get; set; } = default!;

    /// <summary>
    /// Participation id
    /// </summary>
    [Required]
    public int ParticipationId { get; set; }

    /// <summary>
    /// Participation
    /// </summary>
    public Participation Participation = default!;
}
