namespace Scobius.Core.Interfaces;

public interface IEmailService
{
    Task SendEmailVerificationAsync(string toEmail, string displayName, string confirmationLink);
}
