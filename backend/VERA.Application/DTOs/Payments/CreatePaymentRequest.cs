namespace VERA.Application.DTOs.Payments;

public class CreatePaymentRequest
{
    public Guid ListingId { get; set; }

    public Guid? AuctionId { get; set; }
}