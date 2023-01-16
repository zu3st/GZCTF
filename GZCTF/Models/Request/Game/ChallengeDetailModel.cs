namespace CTFServer.Models.Request.Game;

/// <summary>
/// Challenge detail information
/// </summary>
public class ChallengeDetailModel
{
    /// <summary>
    /// Challenge Id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Challenge title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Challenge body
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Challenge tag
    /// </summary>
    public ChallengeTag Tag { get; set; } = ChallengeTag.Misc;

    /// <summary>
    /// Challenge hints
    /// </summary>
    public List<string>? Hints { get; set; }

    /// <summary>
    /// Challenge score
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// Challenge type
    /// </summary>
    public ChallengeType Type { get; set; } = ChallengeType.StaticAttachment;

    /// <summary>
    /// Flag context
    /// </summary>
    public ClientFlagContext Context { get; set; } = default!;

    internal static ChallengeDetailModel FromInstance(Instance instance)
        => new()
        {
            Id = instance.Challenge.Id,
            Content = instance.Challenge.Content,
            Hints = instance.Challenge.Hints,
            Score = instance.Challenge.CurrentScore,
            Tag = instance.Challenge.Tag,
            Title = instance.Challenge.Title,
            Type = instance.Challenge.Type,
            Context = new()
            {
                InstanceEntry = instance.Container?.Entry,
                CloseTime = instance.Container?.ExpectStopAt,
                Url = instance.AttachmentUrl,
                FileSize = instance.Attachment?.FileSize,
            }
        };
}

public class ClientFlagContext
{
    /// <summary>
    /// 题目实例的关闭时间
    /// </summary>
    public DateTimeOffset? CloseTime { get; set; }

    /// <summary>
    /// 题目实例的连接方式
    /// </summary>
    public string? InstanceEntry { get; set; } = null;

    /// <summary>
    /// 附件 Url
    /// </summary>
    public string? Url { get; set; } = null;

    /// <summary>
    /// 附件文件大小
    /// </summary>
    public long? FileSize { get; set; } = null;
}