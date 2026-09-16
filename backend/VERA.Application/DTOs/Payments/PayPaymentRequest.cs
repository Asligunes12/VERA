namespace VERA.Application.DTOs.Payments;

public class PayPaymentRequest
{
    public string? IdempotencyKey { get; set; }
}