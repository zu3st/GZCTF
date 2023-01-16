namespace CTFServer.Models.Internal;

public class CheatCheckInfo
{
    /// <summary>
    /// Check result
    /// </summary>
    public AnswerResult AnswerResult { get; set; } = AnswerResult.WrongAnswer;

    /// <summary>
    /// Related challenge
    /// </summary>
    public Challenge? Challenge { get; set; }

    /// <summary>
    /// Cheating team
    /// </summary>
    public Team? CheatTeam { get; set; }

    /// <summary>
    /// Flag source team
    /// </summary>
    public Team? SourceTeam { get; set; }

    /// <summary>
    /// Cheating user
    /// </summary>
    public UserInfo? CheatUser { get; set; }

    /// <summary>
    /// Flag text
    /// </summary>
    public string? Flag { get; set; }
}