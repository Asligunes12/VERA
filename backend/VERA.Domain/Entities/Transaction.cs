namespace VERA.Domain.Entities;

public class Transaction
{
    public Guid TransactionId { get; set; }

    public Guid? PaymentId { get; set; }

    public Guid UserId { get; set; }

    public string TransactionType { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string? Description { get; set; }

    public string? ProviderTransactionId { get; set; }

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public Payment? Payment { get; set; }

    public User User { get; set; } = null!;
}