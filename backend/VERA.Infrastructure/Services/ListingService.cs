using Microsoft.EntityFrameworkCore;
using VERA.Application.DTOs.Listing;
using VERA.Application.Exceptions;
using VERA.Application.Services;
using VERA.Domain.Entities;
using VERA.Infrastructure.Persistence;

namespace VERA.Infrastructure.Services;

public class ListingService : IListingService
{
    private readonly VeraDbContext _context;

    public ListingService(VeraDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> CreateAsync(
        Guid sellerUserId,
        CreateListingRequest request)
    {
        if (request.Animals is null || request.Animals.Count == 0)
        {
            throw new AppException(
                "İlan en az bir hayvan içermelidir.",
                "ANIMAL_REQUIRED",
                400);
        }

        var validSaleTypes = new[]
        {
            "Sabit Fiyat",
            "Pazarlığa Açık",
            "Canlı Mezat"
        };

        if (!validSaleTypes.Contains(request.SaleType))
        {
            throw new AppException(
                "Geçersiz satış tipi.",
                "INVALID_SALE_TYPE",
                400);
        }

        var sellerExists = await _context.Users
            .AnyAsync(u =>
                u.UserId == sellerUserId &&
                u.IsActive &&
                !u.IsDeleted);

        if (!sellerExists)
        {
            throw new AppException(
                "Satıcı kullanıcı bulunamadı.",
                "SELLER_NOT_FOUND",
                404);
        }

        var listing = new Listing
        {
            ListingId = Guid.NewGuid(),
            SellerUserId = sellerUserId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            SaleType = request.SaleType,
            Status = "Active",
            Location = request.Location.Trim(),
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        foreach (var animalRequest in request.Animals)
        {
            if (animalRequest.Quantity <= 0)
            {
                throw new AppException(
                    "Hayvan miktarı 0'dan büyük olmalıdır.",
                    "INVALID_ANIMAL_QUANTITY",
                    400);
            }

            var animal = new Animal
            {
                AnimalId = Guid.NewGuid(),
                ListingId = listing.ListingId,
                Category = animalRequest.Category.Trim(),
                Breed = animalRequest.Breed?.Trim(),
                AgeMonths = animalRequest.AgeMonths,
                Gender = animalRequest.Gender?.Trim(),
                Quantity = animalRequest.Quantity,
                IsGroupSale = animalRequest.IsGroupSale,
                UnitPrice = animalRequest.UnitPrice,
                TotalPrice = animalRequest.TotalPrice,
                HealthInfo = animalRequest.HealthInfo?.Trim(),
                Location = animalRequest.Location?.Trim(),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            listing.Animals.Add(animal);
        }

        _context.Listings.Add(listing);

        await _context.SaveChangesAsync();

        return listing.ListingId;
    }
    public async Task<List<ListingResponse>> GetAllAsync()
{
    var listings = await _context.Listings
        .AsNoTracking()
        .Include(l => l.Animals)
        .Where(l =>
            !l.IsDeleted &&
            l.Status == "Active")
        .OrderByDescending(l => l.CreatedAt)
        .ToListAsync();

    return listings.Select(l => new ListingResponse
    {
        ListingId = l.ListingId,
        Title = l.Title,
        Description = l.Description,
        SaleType = l.SaleType,
        Status = l.Status,
        Location = l.Location,
        CreatedAt = l.CreatedAt,

        Animals = l.Animals
            .Where(a => !a.IsDeleted)
            .Select(a => new ListingAnimalResponse
            {
                AnimalId = a.AnimalId,
                Category = a.Category,
                Breed = a.Breed,
                AgeMonths = a.AgeMonths,
                Gender = a.Gender,
                Quantity = a.Quantity,
                IsGroupSale = a.IsGroupSale,
                UnitPrice = a.UnitPrice,
                TotalPrice = a.TotalPrice,
                HealthInfo = a.HealthInfo,
                Location = a.Location
            })
            .ToList()
    }).ToList();
}
}