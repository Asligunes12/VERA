namespace VERA.Application.DTOs.Listing;

public class ListingResponse
{
    public Guid ListingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SaleType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public List<ListingAnimalResponse> Animals { get; set; } = [];
}

public class ListingAnimalResponse
{
    public Guid AnimalId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Breed { get; set; }
    public int? AgeMonths { get; set; }
    public string? Gender { get; set; }
    public int Quantity { get; set; }
    public bool IsGroupSale { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? TotalPrice { get; set; }
    public string? HealthInfo { get; set; }
    public string? Location { get; set; }
}