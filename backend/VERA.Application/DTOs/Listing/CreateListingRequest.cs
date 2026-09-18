using System.ComponentModel.DataAnnotations;

namespace VERA.Application.DTOs.Listing;

public class CreateListingRequest
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [StringLength(3000)]
    public string? Description { get; set; }

    [Required]
    [StringLength(30)]
    public string SaleType { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Location { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public List<CreateAnimalRequest> Animals { get; set; } = [];
}