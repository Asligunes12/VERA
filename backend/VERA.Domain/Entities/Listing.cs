namespace VERA.Domain.Entities;

public class Listing
{
    public Guid ListingId { get; set; }

    public Guid SellerUserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string SaleType { get; set; } = "Sabit Fiyat";

    public string Status { get; set; } = "Active";

    public string Location { get; set; } = string.Empty;

    public bool IsDeleted { get; set; } = false;

    public DateTime? DeactivatedAt { get; set; }

    public DateTime? SoldAt { get; set; }

    public DateTime? ExpiredAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public User SellerUser { get; set; } = null!;

    public ICollection<Animal> Animals { get; set; } = new List<Animal>();
}