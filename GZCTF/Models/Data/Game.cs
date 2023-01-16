using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using CTFServer.Models.Request.Edit;
using CTFServer.Utils;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;

namespace CTFServer.Models;

public class Game
{
    [Key]
    [Required]
    public int Id { get; set; }

    /// <summary>
    /// Game title
    /// </summary>
    [Required]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Game signature public key
    /// </summary>
    [Required]
    public string PublicKey { get; set; } = string.Empty;

    /// <summary>
    /// Game signature private key
    /// </summary>
    [Required]
    public string PrivateKey { get; set; } = string.Empty;

    /// <summary>
    /// Whether the game is hidden
    /// </summary>
    [Required]
    public bool Hidden { get; set; } = false;

    /// <summary>
    /// Poster image hash
    /// </summary>
    [MaxLength(64)]
    public string? PosterHash { get; set; }

    /// <summary>
    /// Game summary
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Game details
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Whether to enroll teams without manual review
    /// </summary>
    public bool AcceptWithoutReview { get; set; } = false;

    /// <summary>
    /// Game invite code
    /// </summary>
    public string? InviteCode { get; set; }

    /// <summary>
    /// List of participating organizations
    /// </summary>
    public HashSet<string>? Organizations { get; set; }

    /// <summary>
    /// Limit of team member count, 0 for no limit
    /// </summary>
    public int TeamMemberCountLimit { get; set; } = 0;

    /// <summary>
    /// Limit of concurrent containers per team, 0 for no limit
    /// </summary>
    public int ContainerCountLimit { get; set; } = 3;

    /// <summary>
    ///  Start time (UTC)
    /// </summary>
    [Required]
    [JsonPropertyName("start")]
    public DateTimeOffset StartTimeUTC { get; set; } = DateTimeOffset.FromUnixTimeSeconds(0);

    /// <summary>
    /// End time (UTC)
    /// </summary>
    [Required]
    [JsonPropertyName("end")]
    public DateTimeOffset EndTimeUTC { get; set; } = DateTimeOffset.FromUnixTimeSeconds(0);

    /// <summary>
    /// Writeup submission deadline
    /// </summary>
    [Required]
    [JsonPropertyName("wpddl")]
    public DateTimeOffset WriteupDeadline { get; set; } = DateTimeOffset.FromUnixTimeSeconds(0);

    /// <summary>
    /// Writeup additional notes
    /// </summary>
    [Required]
    [JsonPropertyName("wpnote")]
    public string WriteupNote { get; set; } = string.Empty;

    /// <summary>
    /// First three bloods bonus amount
    /// </summary>
    [Required]
    public BloodBonus BloodBonus { get; set; } = BloodBonus.Default;

    [NotMapped]
    [JsonIgnore]
    public bool IsActive => StartTimeUTC <= DateTimeOffset.Now && DateTimeOffset.Now <= EndTimeUTC;

    #region Db Relationship

    /// <summary>
    /// Game events of this game
    /// </summary>
    [JsonIgnore]
    public List<GameEvent> GameEvents { get; set; } = new();

    /// <summary>
    /// Game notices of this game
    /// </summary>
    [JsonIgnore]
    public List<GameNotice> GameNotices { get; set; } = new();

    /// <summary>
    /// Challenges of this game
    /// </summary>
    [JsonIgnore]
    public List<Challenge> Challenges { get; set; } = new();

    /// <summary>
    /// Submissions of this game
    /// </summary>
    [JsonIgnore]
    public List<Submission> Submissions { get; set; } = new();

    /// <summary>
    /// Participations of this game
    /// </summary>
    [JsonIgnore]
    public HashSet<Participation> Participations { get; set; } = new();

    /// <summary>
    /// Teams competing in this game
    /// </summary>
    [JsonIgnore]
    public ICollection<Team>? Teams { get; set; }

    /// <summary>
    /// Whether the game is in practice mode (accessible after the game ends)
    /// </summary>
    public bool PracticeMode { get; set; } = true;

    #endregion Db Relationship

    [NotMapped]
    public string? PosterUrl => PosterHash is null ? null : $"/assets/{PosterHash}/poster";

    internal void GenerateKeyPair(byte[]? xorkey)
    {
        SecureRandom sr = new();
        Ed25519KeyPairGenerator kpg = new();
        kpg.Init(new Ed25519KeyGenerationParameters(sr));
        AsymmetricCipherKeyPair kp = kpg.GenerateKeyPair();
        Ed25519PrivateKeyParameters privateKey = (Ed25519PrivateKeyParameters)kp.Private;
        Ed25519PublicKeyParameters publicKey = (Ed25519PublicKeyParameters)kp.Public;

        if (xorkey is null)
            PrivateKey = Base64.ToBase64String(privateKey.GetEncoded());
        else
            PrivateKey = Base64.ToBase64String(Codec.Xor(privateKey.GetEncoded(), xorkey));

        PublicKey = Base64.ToBase64String(publicKey.GetEncoded());
    }

    internal string Sign(string str, byte[]? xorkey)
    {
        Ed25519PrivateKeyParameters privateKey;
        if (xorkey is null)
            privateKey = new(Codec.Base64.DecodeToBytes(PrivateKey), 0);
        else
            privateKey = new(Codec.Xor(Codec.Base64.DecodeToBytes(PrivateKey), xorkey), 0);

        return DigitalSignature.GenerateSignature(str, privateKey, SignAlgorithm.Ed25519);
    }

    internal bool Verify(string data, string sign)
    {
        Ed25519PublicKeyParameters publicKey = new(Codec.Base64.DecodeToBytes(PublicKey), 0);

        return DigitalSignature.VerifySignature(data, sign, publicKey, SignAlgorithm.Ed25519);
    }

    internal Game Update(GameInfoModel model)
    {
        Title = model.Title;
        Content = model.Content;
        Summary = model.Summary;
        Hidden = model.Hidden;
        PracticeMode = model.PracticeMode;
        AcceptWithoutReview = model.AcceptWithoutReview;
        InviteCode = model.InviteCode;
        Organizations = model.Organizations ?? Organizations;
        EndTimeUTC = model.EndTimeUTC;
        StartTimeUTC = model.StartTimeUTC;
        WriteupDeadline = model.WriteupDeadline;
        TeamMemberCountLimit = model.TeamMemberCountLimit;
        ContainerCountLimit = model.ContainerCountLimit;
        WriteupNote = model.WriteupNote;
        BloodBonus = BloodBonus.FromValue(model.BloodBonusValue);

        return this;
    }
}
