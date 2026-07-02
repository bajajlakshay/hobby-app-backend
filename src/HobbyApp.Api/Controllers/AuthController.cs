using HobbyApp.Application.Authentication;
using HobbyApp.Application.Authentication.Models;
using HobbyApp.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HobbyApp.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IIdentityService identityService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await identityService.RegisterAsync(request, cancellationToken);
        return result.Succeeded
            ? Ok(new { requiresVerification = true, email = request.Email })
            : BadRequest(new { errors = result.Errors });
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        var result = await identityService.VerifyEmailAsync(request, cancellationToken);
        return result.Succeeded
            ? Ok(result.Value)
            : BadRequest(new { errors = result.Errors });
    }

    [HttpPost("resend-otp")]
    public async Task<IActionResult> ResendOtp(ResendOtpRequest request, CancellationToken cancellationToken)
    {
        var result = await identityService.ResendOtpAsync(request, cancellationToken);
        return result.Succeeded
            ? Ok(new { sent = true, email = request.Email })
            : BadRequest(new { errors = result.Errors });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await identityService.LoginAsync(request, cancellationToken);
        if (!result.Succeeded)
        {
            return Unauthorized(new { errors = result.Errors });
        }

        if (result.Value!.RequiresEmailVerification)
        {
            // Valid credentials, but the email isn't verified — tell the client to
            // route to OTP verification (403 distinguishes this from bad credentials).
            return StatusCode(StatusCodes.Status403Forbidden,
                new { requiresVerification = true, email = request.Email });
        }

        return Ok(result.Value.Tokens);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await identityService.RefreshTokenAsync(request, cancellationToken);
        return result.Succeeded
            ? Ok(result.Value)
            : Unauthorized(new { errors = result.Errors });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await identityService.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);
        return result.Succeeded
            ? NoContent()
            : BadRequest(new { errors = result.Errors });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me([FromServices] ICurrentUser currentUser) =>
        Ok(new { userId = currentUser.UserId, email = currentUser.Email });
}
