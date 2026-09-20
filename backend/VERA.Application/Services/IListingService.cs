using VERA.Application.DTOs.Listing;

namespace VERA.Application.Services;

public interface IListingService
{
    Task<Guid> CreateAsync(
        Guid sellerUserId,
        CreateListingRequest request);

    Task<List<ListingResponse>> GetAllAsync();

    Task<ListingResponse> GetByIdAsync(Guid listingId);
}