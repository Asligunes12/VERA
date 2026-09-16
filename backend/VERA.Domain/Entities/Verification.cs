namespace VERA.Domain.Entities;

public class Verification
{
    public Guid VerificationId { get; set; }

    public Guid UserId { get; set; }

    public bool IdentityVerified { get; set; } = false;

    public bool PhoneVerified { get; set; } = false;

    public string Status { get; set; } = "Pending";

    public string? RejectionReason { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; } = null!;
}