namespace VERA.Domain.Entities;

public class Dispute
{
    public Guid DisputeId { get; set; }

    public Guid PaymentId { get; set; }

    public Guid RaisedByUserId { get; set; }

    public string Reason { get; set; } = string.Empty;

    public string Status { get; set; } = "Open";

    public string? ResolutionNote { get; set; }

    public Guid? ResolvedByUserId { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public Payment Payment { get; set; } = null!;

    public User RaisedByUser { get; set; } = null!;
}