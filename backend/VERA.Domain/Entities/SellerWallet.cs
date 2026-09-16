namespace VERA.Domain.Entities;

public class SellerWallet
{
    public Guid WalletId { get; set; }

    public Guid UserId { get; set; }

    public decimal AvailableBalance { get; set; } = 0;

    public decimal PendingBalance { get; set; } = 0;

    public decimal TotalEarnings { get; set; } = 0;

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; } = null!;
}