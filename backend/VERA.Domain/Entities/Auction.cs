namespace VERA.Domain.Entities;

public class Auction
{
    public Guid AuctionId { get; set; }

    public Guid ListingId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public decimal StartingPrice { get; set; }

    public decimal MinimumBidIncrement { get; set; }

    public string Status { get; set; } = "Active";

    public Guid? WinnerUserId { get; set; }

    public Guid? WinningBidId { get; set; }

    public Guid? SecondHighestBidId { get; set; }

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public Listing Listing { get; set; } = null!;

    public ICollection<Bid> Bids { get; set; } = new List<Bid>();
}