using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VERA.Application.DTOs.Listing;
using VERA.Application.Services;

namespace VERA.API.Controllers;

[ApiController]
[Route("api/listings")]
public class ListingController : ControllerBase
{
    private readonly IListingService _listingService;

    public ListingController(IListingService listingService)
    {
        _listingService = listingService;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateListingRequest request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var listingId = await _listingService.CreateAsync(
            userId,
            request);

        return Ok(new
        {
            listingId,
            message = "İlan başarıyla oluşturuldu."
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var listings = await _listingService.GetAllAsync();

        return Ok(listings);
    }
        [HttpGet("{listingId:guid}")]
    public async Task<IActionResult> GetById(Guid listingId)
    {
        var listing = await _listingService.GetByIdAsync(listingId);

        return Ok(listing);
    }
}