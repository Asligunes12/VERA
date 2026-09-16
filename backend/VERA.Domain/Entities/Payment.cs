namespace VERA.Domain.Entities;

public class Payment
{
    public Guid PaymentId { get; set; }

    public Guid ListingId { get; set; }
    public Guid? AuctionId { get; set; }

    public Guid BuyerId { get; set; }
    public Guid SellerId { get; set; }

    public decimal Amount { get; set; }
    public decimal ServiceFee { get; set; }
    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = "Pending";

    public string? Provider { get; set; }
    public string? ProviderPaymentId { get; set; }

    public DateTime? PaidAt { get; set; }
    public DateTime? ReleasedAt { get; set; }

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Listing Listing { get; set; } = null!;
    public Auction? Auction { get; set; }

    public User Buyer { get; set; } = null!;
    public User Seller { get; set; } = null!;
}