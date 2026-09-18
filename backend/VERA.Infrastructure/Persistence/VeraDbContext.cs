using Microsoft.EntityFrameworkCore;
using VERA.Domain.Entities;

namespace VERA.Infrastructure.Persistence;

public class VeraDbContext : DbContext
{
    public VeraDbContext(DbContextOptions<VeraDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Listing> Listings { get; set; }
    public DbSet<Animal> Animals { get; set; }
    public DbSet<Auction> Auctions { get; set; }
    public DbSet<Bid> Bids { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Dispute> Disputes { get; set; }
    public DbSet<SellerWallet> SellerWallets { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<BankAccount> BankAccounts { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Verification> Verifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.UserId);

            entity.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasIndex(u => u.Email)
                .IsUnique();

            entity.Property(u => u.PhoneNumber)
                .HasMaxLength(30);

            entity.Property(u => u.PasswordHash)
                .IsRequired();

            entity.Property(u => u.Role)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(u => u.TrustScore)
                .HasPrecision(5, 2);
        });
        modelBuilder.Entity<Verification>(entity =>
{
    entity.HasKey(v => v.VerificationId);

    entity.HasOne(v => v.User)
        .WithOne()
        .HasForeignKey<Verification>(v => v.UserId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.HasIndex(v => v.UserId)
        .IsUnique();

    entity.Property(v => v.Status)
        .IsRequired()
        .HasMaxLength(30);

    entity.Property(v => v.RejectionReason)
        .HasMaxLength(500);
});
modelBuilder.Entity<Animal>(entity =>
{
    entity.ToTable("Animals");
    entity.HasKey(a => a.AnimalId);

    entity.Property(a => a.Category)
        .IsRequired()
        .HasMaxLength(30);

    entity.Property(a => a.Breed)
        .HasMaxLength(100);

    entity.Property(a => a.Gender)
        .HasMaxLength(20);

    entity.Property(a => a.HealthInfo)
        .HasMaxLength(1000);

    entity.Property(a => a.Location)
        .HasMaxLength(200);

    entity.Property(a => a.UnitPrice)
        .HasPrecision(18, 2);

    entity.Property(a => a.TotalPrice)
        .HasPrecision(18, 2);

    entity.HasIndex(a => a.ListingId);

    entity.HasIndex(a => a.Category);
});
modelBuilder.Entity<Listing>(entity =>
{
    entity.ToTable("Listings");
    entity.HasKey(l => l.ListingId);

    entity.HasOne(l => l.SellerUser)
        .WithMany()
        .HasForeignKey(l => l.SellerUserId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.HasMany(l => l.Animals)
        .WithOne()
        .HasForeignKey(a => a.ListingId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.Property(l => l.Title)
        .IsRequired()
        .HasMaxLength(200);

    entity.Property(l => l.Description)
        .HasMaxLength(3000);

    entity.Property(l => l.SaleType)
        .IsRequired()
        .HasMaxLength(30);

    entity.Property(l => l.Status)
        .IsRequired()
        .HasMaxLength(30);

    entity.Property(l => l.Location)
        .IsRequired()
        .HasMaxLength(200);

    entity.HasIndex(l => l.SellerUserId);

    entity.HasIndex(l => l.Status);

    entity.HasIndex(l => l.SaleType);
});
modelBuilder.Entity<Auction>(entity =>
{
    entity.ToTable("Auctions");

    entity.HasKey(a => a.AuctionId);

    entity.HasOne(a => a.Listing)
        .WithOne()
        .HasForeignKey<Auction>(a => a.ListingId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.HasIndex(a => a.ListingId)
        .IsUnique();

    entity.Property(a => a.StartingPrice)
        .HasPrecision(18, 2);

    entity.Property(a => a.MinimumBidIncrement)
        .HasPrecision(18, 2);

    entity.Property(a => a.Status)
        .IsRequired()
        .HasMaxLength(30);

    entity.HasIndex(a => a.Status);
});
modelBuilder.Entity<Bid>(entity =>
{
    entity.ToTable("Bids");

    entity.HasKey(b => b.BidId);

    entity.HasOne(b => b.Auction)
        .WithMany(a => a.Bids)
        .HasForeignKey(b => b.AuctionId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(b => b.User)
        .WithMany()
        .HasForeignKey(b => b.UserId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.Property(b => b.Amount)
        .HasPrecision(18, 2);

    entity.HasIndex(b => b.AuctionId);

    entity.HasIndex(b => b.UserId);

    entity.HasIndex(b => new { b.AuctionId, b.Amount });
});
modelBuilder.Entity<Payment>(entity =>
{
    entity.ToTable("Payments");

    entity.HasKey(p => p.PaymentId);

    entity.HasOne(p => p.Listing)
        .WithMany()
        .HasForeignKey(p => p.ListingId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(p => p.Auction)
        .WithOne()
        .HasForeignKey<Payment>(p => p.AuctionId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(p => p.Buyer)
        .WithMany()
        .HasForeignKey(p => p.BuyerId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(p => p.Seller)
        .WithMany()
        .HasForeignKey(p => p.SellerId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.Property(p => p.Amount)
        .HasPrecision(18, 2);

    entity.Property(p => p.ServiceFee)
        .HasPrecision(18, 2);

    entity.Property(p => p.TotalAmount)
        .HasPrecision(18, 2);

    entity.Property(p => p.Status)
        .IsRequired()
        .HasMaxLength(30);

    entity.Property(p => p.Provider)
        .HasMaxLength(50);

    entity.Property(p => p.ProviderPaymentId)
        .HasMaxLength(200);

    entity.HasIndex(p => p.ListingId);
    entity.HasIndex(p => p.AuctionId);
    entity.HasIndex(p => p.BuyerId);
    entity.HasIndex(p => p.SellerId);
    entity.HasIndex(p => p.Status);
});
modelBuilder.Entity<Dispute>(entity =>
{
    entity.ToTable("Disputes");

    entity.HasKey(d => d.DisputeId);

    entity.HasOne(d => d.Payment)
        .WithMany()
        .HasForeignKey(d => d.PaymentId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(d => d.RaisedByUser)
        .WithMany()
        .HasForeignKey(d => d.RaisedByUserId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.Property(d => d.Reason)
        .IsRequired()
        .HasMaxLength(2000);

    entity.Property(d => d.Status)
        .IsRequired()
        .HasMaxLength(30);

    entity.Property(d => d.ResolutionNote)
        .HasMaxLength(2000);

    entity.HasIndex(d => d.PaymentId);
    entity.HasIndex(d => d.RaisedByUserId);
    entity.HasIndex(d => d.Status);
});
modelBuilder.Entity<SellerWallet>(entity =>
{
    entity.ToTable("SellerWallets");

    entity.HasKey(w => w.WalletId);

    entity.HasOne(w => w.User)
        .WithOne()
        .HasForeignKey<SellerWallet>(w => w.UserId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.Property(w => w.AvailableBalance)
        .HasPrecision(18, 2);

    entity.Property(w => w.PendingBalance)
        .HasPrecision(18, 2);

    entity.Property(w => w.TotalEarnings)
        .HasPrecision(18, 2);

    entity.HasIndex(w => w.UserId)
        .IsUnique();
});
modelBuilder.Entity<Transaction>(entity =>
{
    entity.ToTable("Transactions");

    entity.HasKey(t => t.TransactionId);

    entity.HasOne(t => t.Payment)
        .WithMany()
        .HasForeignKey(t => t.PaymentId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(t => t.User)
        .WithMany()
        .HasForeignKey(t => t.UserId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.Property(t => t.TransactionType)
        .IsRequired()
        .HasMaxLength(30);

    entity.Property(t => t.Amount)
        .HasPrecision(18, 2);

    entity.Property(t => t.Description)
        .HasMaxLength(500);

    entity.Property(t => t.ProviderTransactionId)
        .HasMaxLength(200);

    entity.HasIndex(t => t.PaymentId);
    entity.HasIndex(t => t.UserId);
    entity.HasIndex(t => t.TransactionType);
});
modelBuilder.Entity<BankAccount>(entity =>
{
    entity.ToTable("BankAccounts");

    entity.HasKey(b => b.BankAccountId);

    entity.HasOne(b => b.User)
        .WithMany()
        .HasForeignKey(b => b.UserId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.Property(b => b.AccountHolderName)
        .IsRequired()
        .HasMaxLength(200);

    entity.Property(b => b.IBAN)
        .IsRequired()
        .HasMaxLength(34);

    entity.Property(b => b.BankName)
        .IsRequired()
        .HasMaxLength(100);

    entity.HasIndex(b => b.UserId);

    entity.HasIndex(b => new { b.UserId, b.IsDefault });
});
modelBuilder.Entity<RefreshToken>(entity =>
{
    entity.ToTable("RefreshTokens");

    entity.HasKey(r => r.RefreshTokenId);

    entity.HasOne(r => r.User)
        .WithMany()
        .HasForeignKey(r => r.UserId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.Property(r => r.TokenHash)
        .IsRequired()
        .HasMaxLength(128);

    entity.Property(r => r.ReplacedByTokenHash)
        .HasMaxLength(128);

    entity.HasIndex(r => r.UserId);

    entity.HasIndex(r => r.TokenHash)
        .IsUnique();

    entity.HasIndex(r => r.ExpiresAt);
});
    }
}