using Lienzo.Application.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;

namespace Lienzo.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ISystemSettingService _settings;

    public EmailService(ISystemSettingService settings)
    {
        _settings = settings;
    }

    public async Task SendAsync(string to, string subject, string body)
    {
        var host = await _settings.GetValueAsync("EmailSmtpHost") ?? "smtp.gmail.com";
        var portStr = await _settings.GetValueAsync("EmailSmtpPort") ?? "587";
        var username = await _settings.GetValueAsync("EmailUsername");
        var password = await _settings.GetValueAsync("EmailPassword");
        var fromAddress = await _settings.GetValueAsync("EmailFromAddress");
        var fromName = await _settings.GetValueAsync("EmailFromName") ?? "Lienzo";

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(fromAddress))
            throw new InvalidOperationException("Email SMTP configuration is incomplete.");

        if (!int.TryParse(portStr, out var port))
            port = 587;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromAddress));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = body };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(username, password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
