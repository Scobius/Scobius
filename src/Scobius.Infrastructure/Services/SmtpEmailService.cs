using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Scobius.Core.Interfaces;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _config;

    public SmtpEmailService(IConfiguration config) => _config = config;

    public async Task SendEmailVerificationAsync(string toEmail, string displayName, string confirmationLink)
    {
        using var client = new SmtpClient(_config["Smtp:Host"], _config.GetValue<int>("Smtp:Port"));
        var mail = new MailMessage
        {
            From = new MailAddress(_config["Smtp:FromEmail"]!),
            Subject = "Verify your Scobius account",
            Body = $"""
                <h2>Welcome to Scobius, {displayName}!</h2>
                <p>Please verify your email address to get started.</p>
                <a href="{confirmationLink}"
                   style="background:#6366f1;color:white;padding:12px 24px;border-radius:6px;text-decoration:none">
                   Verify Email
                </a>
                <p>This link expires in 24 hours.</p>
                <p>If you didn't create an account, ignore this email.</p>
            """,
            IsBodyHtml = true
        };
        mail.To.Add(toEmail);
        await client.SendMailAsync(mail);
    }
}
