namespace CTFServer.Services.Interface;

public interface IMailSender
{
    /// <summary>
    /// Send Email
    /// </summary>
    /// <param name="subject">Subject</param>
    /// <param name="content">HTML Content</param>
    /// <param name="to">Recipient</param>
    /// <returns>Whether the email was sent successfully</returns>
    public Task<bool> SendEmailAsync(string subject, string content, string to);

    /// <summary>
    /// Send Email with URL
    /// </summary>
    /// <param name="title">Email title</param>
    /// <param name="information">Email body</param>
    /// <param name="btnmsg">Button text</param>
    /// <param name="userName">Username</param>
    /// <param name="email">Email address</param>
    /// <param name="url">URL</param>
    public Task SendUrlAsync(string? title, string? information, string? btnmsg, string? userName, string? email, string? url);

    /// <summary>
    /// Send Email with Email Confirmation URL
    /// </summary>
    /// <param name="userName">Username</param>
    /// <param name="email">Email address</param>
    /// <param name="confirmLink">Confirmation URL</param>
    public bool SendConfirmEmailUrl(string? userName, string? email, string? confirmLink);

    /// <summary>
    /// Send Email with Password Reset URL
    /// </summary>
    /// <param name="userName">Username</param>
    /// <param name="email">Email address</param>
    /// <param name="resetLink">Reset URL</param>
    public bool SendResetPwdUrl(string? userName, string? email, string? resetLink);

    /// <summary>
    /// Send Email with Email Change URL
    /// </summary>
    /// <param name="userName">Username</param>
    /// <param name="email">Email address</param>
    /// <param name="resetLink">Change URL</param>
    public bool SendChangeEmailUrl(string? userName, string? email, string? resetLink);

    /// <summary>
    /// Send Email with Password Reset URL
    /// </summary>
    /// <param name="userName">Username</param>
    /// <param name="email">Email address</param>
    /// <param name="resetLink">Reset URL</param>
    public bool SendResetPasswordUrl(string? userName, string? email, string? resetLink);
}
