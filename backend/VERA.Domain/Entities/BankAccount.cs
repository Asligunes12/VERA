namespace VERA.Domain.Entities;

public class BankAccount
{
    public Guid BankAccountId { get; set; }

    public Guid UserId { get; set; }

    public string AccountHolderName { get; set; } = string.Empty;

    public string IBAN { get; set; } = string.Empty;

    public string BankName { get; set; } = string.Empty;

    public bool IsDefault { get; set; } = false;

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; } = null!;
}