namespace VERA.Domain.Entities;

public class Animal
{
    public Guid AnimalId { get; set; }

    public Guid ListingId { get; set; }

    public string Category { get; set; } = string.Empty;

    public string? Breed { get; set; }

    public int? AgeMonths { get; set; }

    public string? Gender { get; set; }

    public int Quantity { get; set; } = 1;

    public bool IsGroupSale { get; set; } = false;

    public decimal? UnitPrice { get; set; }

    public decimal? TotalPrice { get; set; }

    public string? HealthInfo { get; set; }

    public string? Location { get; set; }

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}