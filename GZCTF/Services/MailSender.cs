using System.Reflection;
using CTFServer.Models.Internal;
using CTFServer.Services.Interface;
using CTFServer.Utils;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CTFServer.Services;

public class MailSender : IMailSender
{
    private readonly EmailConfig? options;
    private readonly ILogger<MailSender> logger;

    public MailSender(IOptions<EmailConfig> options, ILogger<MailSender> logger)
    {
        this.options = options.Value;
        this.logger = logger;
    }

    public async Task<bool> SendEmailAsync(string subject, string content, string to)
    {
        if (options?.SendMailAddress is null ||
            this.options?.Smtp?.Host is null ||
            this.options?.Smtp?.Port is null)
            return true;

        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(options.SendMailAddress, options.SendMailAddress));
        msg.To.Add(new MailboxAddress(to, to));
        msg.Subject = subject;
        msg.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = content };

        try
        {
            using var client = new SmtpClient();

            await client.ConnectAsync(options.Smtp.Host, options.Smtp.Port.Value);
            client.AuthenticationMechanisms.Remove("XOAUTH2");
            await client.AuthenticateAsync(options.UserName, options.Password);
            await client.SendAsync(msg);
            await client.DisconnectAsync(true);

            logger.SystemLog("Sent email to: " + to, TaskStatus.Success, LogLevel.Information);
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while sending email");
            return false;
        }
    }

    public async Task SendUrlAsync(string? title, string? information, string? btnmsg, string? userName, string? email, string? url)
    {
        if (email is null || userName is null || title is null)
        {
            logger.SystemLog("Bad email sending call!", TaskStatus.Fail);
            return;
        }

        string ns = typeof(MailSender).Namespace ?? "CTFServer.Services";
        Assembly asm = typeof(MailSender).Assembly;
        string resourceName = $"{ns}.Assets.URLEmailTemplate.html";
        string emailContent = await
            new StreamReader(asm.GetManifestResourceStream(resourceName)!)
            .ReadToEndAsync();
        emailContent = emailContent
            .Replace("{title}", title)
            .Replace("{information}", information)
            .Replace("{btnmsg}", btnmsg)
            .Replace("{email}", email)
            .Replace("{userName}", userName)
            .Replace("{url}", url)
            .Replace("{nowtime}", DateTimeOffset.UtcNow.ToString("u"));
        if (!await SendEmailAsync(title, emailContent, email))
            logger.SystemLog("Failed to send email!", TaskStatus.Fail);
    }

    private bool SendUrlIfPossible(string? title, string? information, string? btnmsg, string? userName, string? email, string? url)
    {
        if (options?.SendMailAddress is null)
            return false;

        var _ = SendUrlAsync(title, information, btnmsg, userName, email, url);
        return true;
    }

    public bool SendConfirmEmailUrl(string? userName, string? email, string? confirmLink)
        => SendUrlIfPossible("Confirm your email",
            "You need to confirm your email: " + email,
            "Confirm your email", userName, email, confirmLink);

    public bool SendResetPwdUrl(string? userName, string? email, string? resetLink)
        => SendUrlIfPossible("Reset your password",
            "Click the button below to reset your password.",
            "Reset your password", userName, email, resetLink);

    public bool SendChangeEmailUrl(string? userName, string? email, string? resetLink)
        => SendUrlIfPossible("Change your email",
            "Click the button below to change your email.",
            "Change your email", userName, email, resetLink);

    public bool SendResetPasswordUrl(string? userName, string? email, string? resetLink)
        => SendUrlIfPossible("Reset your password",
            "Click the button below to reset your password.",
            "Reset your password", userName, email, resetLink);
}
