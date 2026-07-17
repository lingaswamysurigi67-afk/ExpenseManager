using System.Net;
using System.Net.Mail;

namespace ExpenseManager.Api.Services;

// Minimal SMTP email sender. Configure via the "Smtp" config section
// (Host, Port, User, Password, From, FromName) — set these as env vars in Render.
public class EmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_config["Smtp:Host"]) &&
        !string.IsNullOrWhiteSpace(_config["Smtp:From"]);

    public async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        if (!IsConfigured)
        {
            // Don't hard-fail the request if email isn't set up yet; log so it's visible.
            _logger.LogWarning("SMTP not configured; skipping email to {Email} with subject '{Subject}'.", toEmail, subject);
            return;
        }

        var host = _config["Smtp:Host"]!;
        var port = int.TryParse(_config["Smtp:Port"], out var p) ? p : 587;
        var user = _config["Smtp:User"];
        var password = _config["Smtp:Password"];
        var from = _config["Smtp:From"]!;
        var fromName = _config["Smtp:FromName"] ?? "ExpenseFlow";

        using var message = new MailMessage
        {
            From = new MailAddress(from, fromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(toEmail);

        using var smtp = new SmtpClient(host, port)
        {
            EnableSsl = true,
            Credentials = string.IsNullOrWhiteSpace(user) ? null : new NetworkCredential(user, password)
        };

        await smtp.SendMailAsync(message);
    }
}
