namespace VERA.Application.DTOs.Payments;

public class PaymentResponse
{
    public Guid PaymentId { get; set; }

    public Guid ListingId { get; set; }

    public Guid? AuctionId { get; set; }

    public Guid BuyerId { get; set; }

    public Guid SellerId { get; set; }

    public decimal Amount { get; set; }

    public decimal ServiceFee { get; set; }

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime? PaidAt { get; set; }

    public DateTime? ReleasedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}