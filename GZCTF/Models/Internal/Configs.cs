using System.Text.Json.Serialization;

namespace CTFServer.Models.Internal;

/// <summary>
/// Account Policy
/// </summary>
public class AccountPolicy
{
    /// <summary>
    /// Whether to allow registration
    /// </summary>
    public bool AllowRegister { get; set; } = true;

    /// <summary>
    /// Whether to activate account on registration
    /// </summary>
    public bool ActiveOnRegister { get; set; } = true;

    /// <summary>
    /// Whether to use Google reCAPTCHA
    /// </summary>
    public bool UseGoogleRecaptcha { get; set; } = false;

    /// <summary>
    /// Whether to require email confirmation
    /// </summary>
    public bool EmailConfirmationRequired { get; set; } = false;

    /// <summary>
    /// List of email domains allowed to register
    /// </summary>
    public string EmailDomainList { get; set; } = string.Empty;
}

/// <summary>
/// Global configuration
/// </summary>
public class GlobalConfig
{
    /// <summary>
    /// Platform title
    /// </summary>
    public string Title { get; set; } = "GZ";

    /// <summary>
    /// Platform slogan
    /// </summary>
    public string Slogan { get; set; } = "Hack for fun not for profit";
}

public class SmtpConfig
{
    public string? Host { get; set; } = "127.0.0.1";
    public int? Port { get; set; } = 587;
}

public class EmailConfig
{
    public string? UserName { get; set; } = string.Empty;
    public string? Password { get; set; } = string.Empty;
    public string? SendMailAddress { get; set; } = string.Empty;
    public SmtpConfig? Smtp { get; set; } = new();
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ContainerProviderType
{
    Docker,
    Kubernetes
}

public class ContainerProvider
{
    public ContainerProviderType Type { get; set; } = ContainerProviderType.Docker;
    public string PublicEntry { get; set; } = string.Empty;

    public DockerConfig? DockerConfig { get; set; }
}

public class DockerConfig
{
    public string Uri { get; set; } = string.Empty;
    public bool SwarmMode { get; set; } = false;
}

public class RegistryConfig
{
    public string? ServerAddress { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
}

public class RecaptchaConfig
{
    public string? Secretkey { get; set; }
    public string? SiteKey { get; set; }
    public string VerifyAPIAddress { get; set; } = "https://www.recaptcha.net/recaptcha/api/siteverify";
    public float RecaptchaThreshold { get; set; } = 0.5f;
}