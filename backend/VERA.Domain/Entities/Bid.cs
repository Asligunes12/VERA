namespace VERA.Domain.Entities;

public class Bid
{
    public Guid BidId { get; set; }

    public Guid AuctionId { get; set; }

    public Guid UserId { get; set; }

    public decimal Amount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? WithdrawnAt { get; set; }

    public bool IsWithdrawn { get; set; } = false;

    public bool IsDeleted { get; set; } = false;

    public Auction Auction { get; set; } = null!;

    public User User { get; set; } = null!;
}