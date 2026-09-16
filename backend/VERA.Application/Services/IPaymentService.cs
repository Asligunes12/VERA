using VERA.Application.DTOs.Payments;

namespace VERA.Application.Services;

public interface IPaymentService
{
    Task<PaymentResponse> CreatePaymentAsync(
        Guid buyerId,
        CreatePaymentRequest request);

    Task<PaymentResponse?> GetPaymentAsync(
        Guid paymentId,
        Guid userId);

    Task<PaymentResponse> PayPaymentAsync(
        Guid paymentId,
        Guid buyerId,
        PayPaymentRequest request);

    Task<PaymentResponse> ReleasePaymentAsync(
        Guid paymentId,
        Guid userId);

    Task<PaymentResponse> RefundPaymentAsync(
        Guid paymentId,
        Guid userId,
        RefundPaymentRequest request);

    Task<PaymentResponse> DisputePaymentAsync(
        Guid paymentId,
        Guid userId,
        DisputePaymentRequest request);

    Task<bool> WithdrawAsync(
        Guid userId,
        WithdrawRequest request);
}