using System.Text.Json.Serialization;

namespace CTFServer;

/// <summary>
/// User Permission Level
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Role : byte
{
    /// <summary>
    /// Banned user permission
    /// </summary>
    Banned = 0,

    /// <summary>
    /// Regular user permission
    /// </summary>
    User = 1,

    /// <summary>
    /// Monitor permission, can view submission logs
    /// </summary>
    Monitor = 2,

    /// <summary>
    /// Administrator permission, can view system logs
    /// </summary>
    Admin = 3,
}

/// <summary>
/// Login Response Status
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RegisterStatus : byte
{
    /// <summary>
    /// Successfully registered and logged in
    /// </summary>
    LoggedIn = 0,

    /// <summary>
    /// Waiting for administrator confirmation
    /// </summary>
    AdminConfirmationRequired = 1,

    /// <summary>
    /// Waiting for email confirmation
    /// </summary>
    EmailConfirmationRequired = 2
}

/// <summary>
/// Task completed successfully
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TaskStatus : sbyte
{
    /// <summary>
    /// Task is currently in progress
    /// </summary>
    Pending = -1,

    /// <summary>
    /// Task completed successfully
    /// </summary>
    Success = 0,

    /// <summary>
    /// Task execution failed
    /// </summary>
    Fail = 1,

    /// <summary>
    /// Task encountered duplicate error
    /// </summary>
    Duplicate = 2,

    /// <summary>
    /// Task processing was denied
    /// </summary>
    Denied = 3,

    /// <summary>
    /// Task request not found
    /// </summary>
    NotFound = 4,

    /// <summary>
    /// Task thread is exiting
    /// </summary>
    Exit = 5,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FileType : byte
{
    /// <summary>
    /// No attachment
    /// Indicates that no file is attached to the submission
    /// </summary>
    None = 0,

    /// <summary>
    /// Local file
    /// </summary>
    Local = 1,

    /// <summary>
    /// Remote file
    /// </summary>
    Remote = 2,
}

/// <summary>
/// Container status Type
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ContainerStatus : byte
{
    /// <summary>
    /// Starting
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Running
    /// </summary>
    Running = 1,

    /// <summary>
    /// Destroyed
    /// </summary>
    Destroyed = 2
}

/// <summary>
/// Game Announcement Type
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NoticeType : byte
{
    /// <summary>
    /// General announcement
    /// </summary>
    Normal = 0,

    /// <summary>
    /// First blood announcement
    /// </summary>
    FirstBlood = 1,

    /// <summary>
    /// Second blood announcement
    /// </summary>
    SecondBlood = 2,

    /// <summary>
    /// Third blood announcement
    /// </summary>
    ThirdBlood = 3,

    /// <summary>
    /// New hint announcement
    /// </summary>
    NewHint = 4,

    /// <summary>
    /// New challenge announcement
    /// </summary>
    NewChallenge = 5,
}

public static class SubmissionTypeExtensions
{
    public static string ToBloodString(this SubmissionType type)
        => type switch
        {
            SubmissionType.FirstBlood => "First Blood",
            SubmissionType.SecondBlood => "Second Blood",
            SubmissionType.ThirdBlood => "Third Blood",
            _ => throw new ArgumentException(type.ToString(), nameof(type))
        };
}

/// <summary>
/// Game Event Type
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EventType : byte
{
    /// <summary>
    /// General event
    /// </summary>
    Normal = 0,

    /// <summary>
    /// Container Start Event
    /// </summary>
    ContainerStart = 1,

    /// <summary>
    /// Container Destroy Event
    /// </summary>
    ContainerDestroy = 2,

    /// <summary>
    /// Flag Submit Event
    /// </summary>
    FlagSubmit = 3,

    /// <summary>
    /// Cheat Detected Event
    /// </summary>
    CheatDetected = 4
}

/// <summary>
/// Submission Type
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SubmissionType : byte
{
    /// <summary>
    /// Unaccepted
    /// </summary>
    Unaccepted = 0,

    /// <summary>
    /// First Blood
    /// </summary>
    FirstBlood = 1,

    /// <summary>
    /// Second Blood
    /// </summary>
    SecondBlood = 2,

    /// <summary>
    /// Third Blood
    /// </summary>
    ThirdBlood = 3,

    /// <summary>
    /// Solved
    /// </summary>
    Normal = 4,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ParticipationStatus : byte
{
    /// <summary>
    /// Registered
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Accepted
    /// </summary>
    Accepted = 1,

    /// <summary>
    /// Rejected
    /// </summary>
    Denied = 2,

    /// <summary>
    /// Cancelled
    /// </summary>
    Forfeited = 3,

    /// <summary>
    /// Not submitted
    /// </summary>
    Unsubmitted = 4,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChallengeType : byte
{
    /// <summary>
    /// Static attachment challenge
    /// All teams use the same attachment and the same flag
    /// </summary>
    StaticAttachment = 0b00,

    /// <summary>
    /// Static container challenge
    /// All teams use the same container and the same flag
    /// </summary>
    StaticContainer = 0b01,

    /// <summary>
    /// Dynamic attachment challenge
    /// Randomly distributed attachments with individual flags
    /// </summary>
    DynamicAttachment = 0b10,

    /// <summary>
    /// Dynamic container challenge
    /// Randomly distributed containers with individual flags passed as environment variables
    /// </summary>
    DynamicContainer = 0b11
}

public static class ChallengeTypeExtensions
{
    /// <summary>
    /// Whether it is a static challenge
    /// </summary>
    public static bool IsStatic(this ChallengeType type) => ((byte)type & 0b10) == 0;

    /// <summary>
    /// Whether it is a dynamic challenge
    /// </summary>
    public static bool IsDynamic(this ChallengeType type) => ((byte)type & 0b10) != 0;

    /// <summary>
    /// Whether it is an attachment challenge
    /// </summary>
    public static bool IsAttachment(this ChallengeType type) => ((byte)type & 0b01) == 0;

    /// <summary>
    /// Whether it is a container challenge
    /// </summary>
    public static bool IsContainer(this ChallengeType type) => ((byte)type & 0b01) != 0;
}

/// <summary>
/// Challenge Tags
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChallengeTag : byte
{
    Misc = 0,
    Crypto = 1,
    Pwn = 2,
    Web = 3,
    Reverse = 4,
    Blockchain = 5,
    Forensics = 6,
    Hardware = 7,
    Mobile = 8,
    PPC = 9
}

/// <summary>
/// Assessment Result
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AnswerResult : byte
{
    /// <summary>
    /// Successfully submitted
    /// </summary>
    FlagSubmitted = 0,

    /// <summary>
    /// Correct answer
    /// </summary>
    Accepted = 1,

    /// <summary>
    /// Incorrect answer
    /// </summary>
    WrongAnswer = 2,

    /// <summary>
    /// Submitted challenge instance not found
    /// </summary>
    NotFound = 3,

    /// <summary>
    /// Cheating detected
    /// </summary>
    CheatDetected = 4
}

public static class AnswerResultExtensions
{
    public static string ToShortString(this AnswerResult result)
        => result switch
        {
            AnswerResult.FlagSubmitted => "Flag Submitted",
            AnswerResult.Accepted => "Accepted",
            AnswerResult.WrongAnswer => "Wrong Answer",
            AnswerResult.NotFound => "Challenge instance not found",
            AnswerResult.CheatDetected => "Cheating detected",
            _ => "??"
        };
}