using System.ComponentModel.DataAnnotations;

namespace HobbyApp.Application.Authentication.Models;

public sealed record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required, StringLength(32, MinimumLength = 3), RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "Username can only contain letters, numbers, dots, underscores, and hyphens.")] string Username,
    [Required, MinLength(8)] string Password);

public sealed record LoginRequest(
    [Required] string Identifier,
    [Required] string Password);

public sealed record RefreshTokenRequest(
    [Required] string RefreshToken);

public sealed record VerifyEmailRequest(
    [Required, EmailAddress] string Email,
    [Required, StringLength(6, MinimumLength = 6)] string Code);

public sealed record ResendOtpRequest(
    [Required, EmailAddress] string Email);
