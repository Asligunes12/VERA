using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VERA.Application.DTOs.Payments;
using VERA.Application.Services;
using Microsoft.AspNetCore.Authorization;

namespace VERA.API.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost]
    public async Task<ActionResult<PaymentResponse>> CreatePayment(
        [FromBody] CreatePaymentRequest request)
    {
        var buyerId = GetUserId();

        var result = await _paymentService.CreatePaymentAsync(
            buyerId,
            request);

        return Ok(result);
    }

    [HttpGet("{paymentId:guid}")]
    public async Task<ActionResult<PaymentResponse>> GetPayment(
        Guid paymentId)
    {
        var userId = GetUserId();

        var result = await _paymentService.GetPaymentAsync(
            paymentId,
            userId);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost("{paymentId:guid}/pay")]
    public async Task<ActionResult<PaymentResponse>> PayPayment(
        Guid paymentId,
        [FromBody] PayPaymentRequest request)
    {
        var buyerId = GetUserId();

        var result = await _paymentService.PayPaymentAsync(
            paymentId,
            buyerId,
            request);

        return Ok(result);
    }

    [HttpPost("{paymentId:guid}/release")]
    public async Task<ActionResult<PaymentResponse>> ReleasePayment(
        Guid paymentId)
    {
        var userId = GetUserId();

        var result = await _paymentService.ReleasePaymentAsync(
            paymentId,
            userId);

        return Ok(result);
    }

    [HttpPost("{paymentId:guid}/refund")]
    public async Task<ActionResult<PaymentResponse>> RefundPayment(
        Guid paymentId,
        [FromBody] RefundPaymentRequest request)
    {
        var userId = GetUserId();

        var result = await _paymentService.RefundPaymentAsync(
            paymentId,
            userId,
            request);

        return Ok(result);
    }

    [HttpPost("{paymentId:guid}/dispute")]
    public async Task<ActionResult<PaymentResponse>> DisputePayment(
        Guid paymentId,
        [FromBody] DisputePaymentRequest request)
    {
        var userId = GetUserId();

        var result = await _paymentService.DisputePaymentAsync(
            paymentId,
            userId,
            request);

        return Ok(result);
    }

    [HttpPost("withdraw")]
    public async Task<ActionResult<bool>> Withdraw(
        [FromBody] WithdrawRequest request)
    {
        var userId = GetUserId();

        var result = await _paymentService.WithdrawAsync(
            userId,
            request);

        return Ok(result);
    }

    private Guid GetUserId()
{
    var claim = User.FindFirst(ClaimTypes.NameIdentifier);

    if (claim is null || !Guid.TryParse(claim.Value, out var userId))
        throw new UnauthorizedAccessException(
            "Kullanıcı kimliği bulunamadı.");

    return userId;
}
}