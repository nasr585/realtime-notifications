using Microsoft.AspNetCore.Mvc;
using RealtimeNotifications.Api.Services;

namespace RealtimeNotifications.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(DemoTokenService tokenService) : ControllerBase
{
    /// <summary>
    /// Demo-only token issuer. It exists so the sample client can authenticate
    /// without standing up an identity provider.
    /// </summary>
    [HttpPost("demo-token")]
    public IActionResult IssueDemoToken([FromBody] DemoTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            return BadRequest(new { error = "userId is required." });
        }

        var token = tokenService.IssueToken(request.UserId.Trim(), request.DisplayName);
        return Ok(new { token });
    }
}

public record DemoTokenRequest(string UserId, string? DisplayName);
