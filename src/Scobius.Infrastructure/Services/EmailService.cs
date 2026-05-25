using Scobius.Core.Interfaces;
using Resend;
using Microsoft.Extensions.Configuration;

namespace Scobius.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IResend _client;
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config, IResend resend)
    {
        _config = config;
        _client = resend;
    }

    public async Task SendEmailVerificationAsync(string toEmail, string displayName, string confirmationLink)
    {
        var message = new EmailMessage
        {
            From = _config["Resend:FromEmail"] ?? "Acme <onboarding@resend.dev>",
            To = [toEmail],
            Subject = "Verify your Scobius account",
            HtmlBody = $"""
                <h2>Welcome to Scobius, {displayName}!</h2>
                <p>Please verify your email address to get started.</p>
                <a href="{confirmationLink}"
                   style="background:#6366f1;color:white;padding:12px 24px;border-radius:6px;text-decoration:none">
                   Verify Email
                </a>
                <p>This link expires in 24 hours.</p>
                <p>If you didn't create an account, ignore this email.</p>
            """
        };

        await _client.EmailSendAsync(message);
    }
}
