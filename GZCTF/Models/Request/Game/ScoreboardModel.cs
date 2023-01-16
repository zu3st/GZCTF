using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CTFServer.Models.Request.Info;
using CTFServer.Utils;
using MemoryPack;

namespace CTFServer.Models.Request.Game;

/// <summary>
/// Leaderboard
/// </summary>
[MemoryPackable]
public partial class ScoreboardModel
{
    /// <summary>
    /// Update time (UTC)
    /// </summary>
    [Required]
    public DateTimeOffset UpdateTimeUTC { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// First blood bonus
    /// </summary>
    [Required]
    [JsonPropertyName("bloodBonus")]
    public long BloodBonusValue { get; set; } = BloodBonus.DefaultValue;

    /// <summary>
    /// Top 10 timeline
    /// </summary>
    public Dictionary<string, IEnumerable<TopTimeLine>> TimeLines { get; set; } = default!;

    /// <summary>
    /// Team information
    /// </summary>
    public IEnumerable<ScoreboardItem> Items { get; set; } = default!;

    /// <summary>
    /// Challenge information
    /// </summary>
    public Dictionary<ChallengeTag, IEnumerable<ChallengeInfo>> Challenges { get; set; } = default!;
}

[MemoryPackable]
public partial class TopTimeLine
{
    /// <summary>
    /// Team id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Team name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Timeline
    /// </summary>
    public IEnumerable<TimeLine> Items { get; set; } = default!;
}

[MemoryPackable]
public partial class TimeLine
{
    /// <summary>
    /// Time
    /// </summary>
    public DateTimeOffset Time { get; set; }

    /// <summary>
    /// Sccore
    /// </summary>
    public int Score { get; set; }
}

[MemoryPackable]
public partial class ScoreboardItem
{
    /// <summary>
    /// Team id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Team name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Organization
    /// </summary>
    public string? Organization { get; set; }

    /// <summary>
    /// Team avatar
    /// </summary>
    public string? Avatar { get; set; } = string.Empty;

    /// <summary>
    /// Score
    /// </summary>
    public int Score => Challenges.Sum(c => c.Score);

    /// <summary>
    /// Rank
    /// </summary>
    public int Rank { get; set; }

    /// <summary>
    /// 参赛所属组织排名
    /// </summary>
    public int? OrganizationRank { get; set; }

    /// <summary>
    /// Number of solved challenges
    /// </summary>
    public int SolvedCount { get; set; }

    /// <summary>
    /// Last submission time
    /// </summary>
    public DateTimeOffset LastSubmissionTime { get; set; }

    /// <summary>
    /// Challenge list
    /// </summary>
    public IEnumerable<ChallengeItem> Challenges { get; set; } = Array.Empty<ChallengeItem>();
}

[MemoryPackable]
public partial class ChallengeItem
{
    /// <summary>
    /// Challenge id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Challenge score
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// Unsolved, first blood, second blood, third blood or other
    /// </summary>
    [JsonPropertyName("type")]
    public SubmissionType Type { get; set; }

    /// <summary>
    /// Solver name
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Time of challenge submission, in order to calculate the timeline
    /// </summary>
    [JsonPropertyName("time")]
    public DateTimeOffset? SubmitTimeUTC { get; set; }
}

[MemoryPackable]
public partial class ChallengeInfo
{
    /// <summary>
    /// Challenge id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Challenge title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Challenge tag
    /// </summary>
    public ChallengeTag Tag { get; set; }

    /// <summary>
    /// Challenge score
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// Number of teams solved
    /// </summary>
    [JsonPropertyName("solved")]
    public int SolvedCount { get; set; }

    /// <summary>
    /// First blood, second blood, third blood
    /// </summary>
    public Blood?[] Bloods { get; set; } = default!;
}

[MemoryPackable]
public partial class Blood
{
    /// <summary>
    /// Team id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Team name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Team avatar
    /// </summary>
    public string? Avatar { get; set; } = string.Empty;

    /// <summary>
    /// Challenge submission time
    /// </summary>
    public DateTimeOffset? SubmitTimeUTC { get; set; }
}
