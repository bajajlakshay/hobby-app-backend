namespace HobbyApp.Infrastructure.Email;

/// <summary>
/// SMTP configuration. Provider-agnostic: works with Gmail, Brevo, Resend, SES, etc.
/// In production, supply values via environment variables (EmailSettings__Host, ...).
/// If <see cref="Host"/> is empty, emails are logged instead of sent (local dev).
/// </summary>
public sealed class EmailSettings
{
    public const string SectionName = "EmailSettings";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "HobbyApp";

    /// <summary>Use STARTTLS (typical for port 587). Set false + port 465 for implicit SSL.</summary>
    public bool UseStartTls { get; set; } = true;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Host);
}
