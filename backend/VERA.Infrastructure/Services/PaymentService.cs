using Microsoft.EntityFrameworkCore;
using VERA.Application.DTOs.Payments;
using VERA.Application.Services;
using VERA.Domain.Entities;
using VERA.Infrastructure.Persistence;

namespace VERA.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly VeraDbContext _context;

    public PaymentService(VeraDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentResponse> CreatePaymentAsync(
    Guid buyerId,
    CreatePaymentRequest request)
{
    var listing = await _context.Listings
        .Include(l => l.Animals)
        .FirstOrDefaultAsync(l =>
            l.ListingId == request.ListingId &&
            !l.IsDeleted &&
            l.Status == "Active");

    if (listing is null)
        throw new InvalidOperationException("İlan bulunamadı veya aktif değil.");

    if (listing.SellerUserId == buyerId)
        throw new InvalidOperationException(
            "Kendi ilanınız için ödeme oluşturamazsınız.");

    decimal amount;

    if (listing.SaleType == "Fixed")
    {
        amount = listing.Animals.Sum(a =>
            a.TotalPrice ??
            ((a.UnitPrice ?? 0) * a.Quantity));
    }
    else if (listing.SaleType == "Auction")
    {
        if (!request.AuctionId.HasValue)
            throw new InvalidOperationException(
                "Mezat ödemesi için AuctionId gereklidir.");

        var auction = await _context.Auctions
            .Include(a => a.Bids)
            .FirstOrDefaultAsync(a =>
                a.AuctionId == request.AuctionId.Value &&
                a.ListingId == listing.ListingId &&
                !a.IsDeleted);

        if (auction is null)
            throw new InvalidOperationException("Mezat bulunamadı.");

        if (auction.WinnerUserId != buyerId)
            throw new InvalidOperationException(
                "Bu mezatın kazananı siz değilsiniz.");

        var winningBid = auction.Bids
            .Where(b => !b.IsDeleted && !b.IsWithdrawn)
            .OrderByDescending(b => b.Amount)
            .FirstOrDefault(b => b.UserId == buyerId);

        if (winningBid is null)
            throw new InvalidOperationException(
                "Geçerli kazanan teklif bulunamadı.");

        amount = winningBid.Amount;
    }
    else
    {
        throw new InvalidOperationException(
            "Bu satış türü için ödeme akışı henüz hazır değil.");
    }

    if (amount <= 0)
        throw new InvalidOperationException(
            "Ödeme tutarı geçersiz.");

    var existingPayment = await _context.Payments
        .FirstOrDefaultAsync(p =>
            p.ListingId == listing.ListingId &&
            p.BuyerId == buyerId &&
            !p.IsDeleted &&
            p.Status != "Cancelled" &&
            p.Status != "Refunded");

    if (existingPayment is not null)
        throw new InvalidOperationException(
            "Bu satış için zaten bir ödeme kaydı bulunmaktadır.");

    const decimal serviceFeeRate = 0.01m;

    var serviceFee = Math.Round(
        amount * serviceFeeRate,
        2);

    var totalAmount = amount + serviceFee;

    var payment = new Payment
    {
        PaymentId = Guid.NewGuid(),
        ListingId = listing.ListingId,
        AuctionId = request.AuctionId,
        BuyerId = buyerId,
        SellerId = listing.SellerUserId,
        Amount = amount,
        ServiceFee = serviceFee,
        TotalAmount = totalAmount,
        Status = "Pending",
        CreatedAt = DateTime.UtcNow
    };

    _context.Payments.Add(payment);

    await _context.SaveChangesAsync();

    return new PaymentResponse
    {
        PaymentId = payment.PaymentId,
        ListingId = payment.ListingId,
        AuctionId = payment.AuctionId,
        BuyerId = payment.BuyerId,
        SellerId = payment.SellerId,
        Amount = payment.Amount,
        ServiceFee = payment.ServiceFee,
        TotalAmount = payment.TotalAmount,
        Status = payment.Status,
        PaidAt = payment.PaidAt,
        ReleasedAt = payment.ReleasedAt,
        CreatedAt = payment.CreatedAt
    };
}

    public async Task<PaymentResponse?> GetPaymentAsync(
    Guid paymentId,
    Guid userId)
{
    var payment = await _context.Payments
        .AsNoTracking()
        .FirstOrDefaultAsync(p =>
            p.PaymentId == paymentId &&
            !p.IsDeleted &&
            (p.BuyerId == userId || p.SellerId == userId));

    if (payment is null)
        return null;

    return new PaymentResponse
    {
        PaymentId = payment.PaymentId,
        ListingId = payment.ListingId,
        AuctionId = payment.AuctionId,
        BuyerId = payment.BuyerId,
        SellerId = payment.SellerId,
        Amount = payment.Amount,
        ServiceFee = payment.ServiceFee,
        TotalAmount = payment.TotalAmount,
        Status = payment.Status,
        PaidAt = payment.PaidAt,
        ReleasedAt = payment.ReleasedAt,
        CreatedAt = payment.CreatedAt
    };
}

    public async Task<PaymentResponse> PayPaymentAsync(
    Guid paymentId,
    Guid buyerId,
    PayPaymentRequest request)
{
    if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
        throw new InvalidOperationException(
            "IdempotencyKey gereklidir.");

    var payment = await _context.Payments
        .FirstOrDefaultAsync(p =>
            p.PaymentId == paymentId &&
            !p.IsDeleted);

    if (payment is null)
        throw new InvalidOperationException(
            "Ödeme bulunamadı.");

    if (payment.BuyerId != buyerId)
        throw new InvalidOperationException(
            "Bu ödeme için yetkiniz bulunmamaktadır.");

    if (payment.Status == "Paid" ||
        payment.Status == "Held" ||
        payment.Status == "Released")
    {
        throw new InvalidOperationException(
            "Bu ödeme daha önce işleme alınmış.");
    }

    if (payment.Status != "Pending")
        throw new InvalidOperationException(
            $"Ödeme şu anda '{payment.Status}' durumunda ve işleme alınamaz.");

    payment.Status = "Processing";
    payment.UpdatedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync();

    return new PaymentResponse
    {
        PaymentId = payment.PaymentId,
        ListingId = payment.ListingId,
        AuctionId = payment.AuctionId,
        BuyerId = payment.BuyerId,
        SellerId = payment.SellerId,
        Amount = payment.Amount,
        ServiceFee = payment.ServiceFee,
        TotalAmount = payment.TotalAmount,
        Status = payment.Status,
        PaidAt = payment.PaidAt,
        ReleasedAt = payment.ReleasedAt,
        CreatedAt = payment.CreatedAt
    };
}

   public async Task<PaymentResponse> ReleasePaymentAsync(
    Guid paymentId,
    Guid userId)
{
    var payment = await _context.Payments
        .FirstOrDefaultAsync(p =>
            p.PaymentId == paymentId &&
            !p.IsDeleted);

    if (payment is null)
        throw new InvalidOperationException(
            "Ödeme bulunamadı.");

    if (payment.BuyerId != userId)
        throw new InvalidOperationException(
            "Bu ödeme için parayı serbest bırakma yetkiniz bulunmamaktadır.");

    if (payment.Status != "Held")
        throw new InvalidOperationException(
            "Sadece beklemede tutulan ödemeler serbest bırakılabilir.");

    if (payment.ReleasedAt.HasValue)
        throw new InvalidOperationException(
            "Bu ödeme daha önce serbest bırakılmış.");

    payment.Status = "Released";
    payment.ReleasedAt = DateTime.UtcNow;
    payment.UpdatedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync();

    return new PaymentResponse
    {
        PaymentId = payment.PaymentId,
        ListingId = payment.ListingId,
        AuctionId = payment.AuctionId,
        BuyerId = payment.BuyerId,
        SellerId = payment.SellerId,
        Amount = payment.Amount,
        ServiceFee = payment.ServiceFee,
        TotalAmount = payment.TotalAmount,
        Status = payment.Status,
        PaidAt = payment.PaidAt,
        ReleasedAt = payment.ReleasedAt,
        CreatedAt = payment.CreatedAt
    };
}

    public async Task<PaymentResponse> RefundPaymentAsync(
    Guid paymentId,
    Guid userId,
    RefundPaymentRequest request)
{
    if (string.IsNullOrWhiteSpace(request.Reason))
        throw new InvalidOperationException(
            "İade nedeni gereklidir.");

    var payment = await _context.Payments
        .FirstOrDefaultAsync(p =>
            p.PaymentId == paymentId &&
            !p.IsDeleted);

    if (payment is null)
        throw new InvalidOperationException(
            "Ödeme bulunamadı.");

    if (payment.BuyerId != userId)
        throw new InvalidOperationException(
            "Bu ödeme için iade talebi oluşturma yetkiniz bulunmamaktadır.");

    if (payment.Status == "Refunded")
        throw new InvalidOperationException(
            "Bu ödeme daha önce iade edilmiş.");

    if (payment.Status != "Paid" &&
        payment.Status != "Held")
        throw new InvalidOperationException(
            "Bu ödeme şu anda iade edilebilir durumda değil.");

    payment.Status = "Refunded";
    payment.UpdatedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync();

    return new PaymentResponse
    {
        PaymentId = payment.PaymentId,
        ListingId = payment.ListingId,
        AuctionId = payment.AuctionId,
        BuyerId = payment.BuyerId,
        SellerId = payment.SellerId,
        Amount = payment.Amount,
        ServiceFee = payment.ServiceFee,
        TotalAmount = payment.TotalAmount,
        Status = payment.Status,
        PaidAt = payment.PaidAt,
        ReleasedAt = payment.ReleasedAt,
        CreatedAt = payment.CreatedAt
    };
}

    public async Task<PaymentResponse> DisputePaymentAsync(
    Guid paymentId,
    Guid userId,
    DisputePaymentRequest request)
{
    if (string.IsNullOrWhiteSpace(request.Reason))
        throw new InvalidOperationException(
            "Uyuşmazlık nedeni gereklidir.");

    var payment = await _context.Payments
        .FirstOrDefaultAsync(p =>
            p.PaymentId == paymentId &&
            !p.IsDeleted);

    if (payment is null)
        throw new InvalidOperationException(
            "Ödeme bulunamadı.");

    if (payment.BuyerId != userId &&
        payment.SellerId != userId)
        throw new InvalidOperationException(
            "Bu ödeme için uyuşmazlık oluşturma yetkiniz bulunmamaktadır.");

    if (payment.Status == "Disputed")
        throw new InvalidOperationException(
            "Bu ödeme için zaten açık bir uyuşmazlık bulunmaktadır.");

    if (payment.Status != "Paid" &&
        payment.Status != "Held")
        throw new InvalidOperationException(
            "Bu ödeme şu anda uyuşmazlığa uygun durumda değil.");

    var existingDispute = await _context.Disputes
        .FirstOrDefaultAsync(d =>
            d.PaymentId == paymentId &&
            !d.IsDeleted &&
            d.Status != "Resolved" &&
            d.Status != "Rejected");

    if (existingDispute is not null)
        throw new InvalidOperationException(
            "Bu ödeme için zaten aktif bir uyuşmazlık bulunmaktadır.");

    var dispute = new Dispute
    {
        DisputeId = Guid.NewGuid(),
        PaymentId = payment.PaymentId,
        RaisedByUserId = userId,
        Reason = request.Reason.Trim(),
        Status = "Open",
        CreatedAt = DateTime.UtcNow
    };

    payment.Status = "Disputed";
    payment.UpdatedAt = DateTime.UtcNow;

    _context.Disputes.Add(dispute);

    await _context.SaveChangesAsync();

    return new PaymentResponse
    {
        PaymentId = payment.PaymentId,
        ListingId = payment.ListingId,
        AuctionId = payment.AuctionId,
        BuyerId = payment.BuyerId,
        SellerId = payment.SellerId,
        Amount = payment.Amount,
        ServiceFee = payment.ServiceFee,
        TotalAmount = payment.TotalAmount,
        Status = payment.Status,
        PaidAt = payment.PaidAt,
        ReleasedAt = payment.ReleasedAt,
        CreatedAt = payment.CreatedAt
    };
}
   public async Task<bool> WithdrawAsync(
    Guid userId,
    WithdrawRequest request)
{
    if (request.Amount <= 0)
        throw new InvalidOperationException(
            "Para çekme tutarı sıfırdan büyük olmalıdır.");

    var wallet = await _context.SellerWallets
        .FirstOrDefaultAsync(w =>
            w.UserId == userId &&
            !w.IsDeleted);

    if (wallet is null)
        throw new InvalidOperationException(
            "Kullanıcı cüzdanı bulunamadı.");

    if (wallet.AvailableBalance < request.Amount)
        throw new InvalidOperationException(
            "Kullanılabilir bakiye para çekme işlemi için yetersiz.");

    var bankAccount = await _context.BankAccounts
        .FirstOrDefaultAsync(b =>
            b.UserId == userId &&
            b.IsDefault &&
            !b.IsDeleted);

    if (bankAccount is null)
        throw new InvalidOperationException(
            "Varsayılan banka hesabı bulunamadı.");

    wallet.AvailableBalance -= request.Amount;
    wallet.UpdatedAt = DateTime.UtcNow;

    var transaction = new Transaction
    {
        TransactionId = Guid.NewGuid(),
        PaymentId = null,
        UserId = userId,
        TransactionType = "Withdrawal",
        Amount = request.Amount,
        Description = "Banka hesabına para çekme talebi.",
        CreatedAt = DateTime.UtcNow
    };

    _context.Transactions.Add(transaction);

    await _context.SaveChangesAsync();

    return true;
}
}