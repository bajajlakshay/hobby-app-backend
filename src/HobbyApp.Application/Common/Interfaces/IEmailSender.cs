namespace HobbyApp.Application.Common.Interfaces;

/// <summary>
/// Sends transactional emails. Implemented in Infrastructure over SMTP (MailKit).
/// </summary>
public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
