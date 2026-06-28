using System.ComponentModel.DataAnnotations;

namespace HobbyApp.Application.Authentication.Models;

public sealed record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password);

public sealed record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public sealed record RefreshTokenRequest(
    [Required] string RefreshToken);
