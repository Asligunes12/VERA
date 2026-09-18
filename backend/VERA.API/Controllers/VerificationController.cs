using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VERA.Application.DTOs.Verification;
using VERA.Application.Services;

namespace VERA.API.Controllers;

[ApiController]
[Route("api/verification")]
[Authorize]
public class VerificationController : ControllerBase
{
    private readonly IVerificationService _verificationService;

    public VerificationController(
        IVerificationService verificationService)
    {
        _verificationService = verificationService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateVerification(
        [FromBody] CreateVerificationRequest request)
    {
        var userId = GetUserId();

        await _verificationService.CreateVerificationAsync(
            userId,
            request);

        return Ok(new
        {
            message = "Doğrulama başvurusu oluşturuldu."
        });
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var userId = GetUserId();

        var result = await _verificationService
            .GetStatusAsync(userId);

        return Ok(result);
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException(
                "Geçersiz kullanıcı kimliği.");
        }

        return userId;
    }
}