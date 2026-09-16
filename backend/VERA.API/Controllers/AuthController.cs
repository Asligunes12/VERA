using Microsoft.AspNetCore.Mvc;
using VERA.Application.DTOs.Auth;
using VERA.Application.Services;

namespace VERA.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(
        [FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(result);
    }
    [HttpPost("refresh")]
public async Task<ActionResult<AuthResponse>> Refresh(
    [FromBody] RefreshTokenRequest request)
{
    var result = await _authService.RefreshAsync(request.RefreshToken);
    return Ok(result);
}
 [HttpPost("logout")]
public async Task<IActionResult> Logout(
    [FromBody] RefreshTokenRequest request)
{
    await _authService.LogoutAsync(request.RefreshToken);

    return Ok(new
    {
        message = "Başarıyla çıkış yapıldı."
    });
}
}