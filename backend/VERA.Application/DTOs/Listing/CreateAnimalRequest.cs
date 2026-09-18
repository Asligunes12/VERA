using System.ComponentModel.DataAnnotations;

namespace VERA.Application.DTOs.Listing;

public class CreateAnimalRequest
{
    [Required]
    [StringLength(50)]
    public string Category { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Breed { get; set; }

    [Range(0, 600)]
    public int? AgeMonths { get; set; }

    [StringLength(20)]
    public string? Gender { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;

    public bool IsGroupSale { get; set; }

    public decimal? UnitPrice { get; set; }

    public decimal? TotalPrice { get; set; }

    [StringLength(1000)]
    public string? HealthInfo { get; set; }

    [StringLength(200)]
    public string? Location { get; set; }
}