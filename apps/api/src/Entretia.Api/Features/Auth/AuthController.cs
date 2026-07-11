using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Entretia.Api.Features.Auth;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(AuthService auth) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        var result = await auth.RegisterAsync(request, ct);
        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return StatusCode(StatusCodes.Status201Created, result.Response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var result = await auth.LoginAsync(request, ct);
        if (!result.Success)
        {
            return Unauthorized(new { error = result.Error });
        }

        return Ok(result.Response);
    }
}
