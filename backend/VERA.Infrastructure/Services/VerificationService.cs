using Microsoft.EntityFrameworkCore;
using VERA.Application.DTOs.Verification;
using VERA.Application.Exceptions;
using VERA.Application.Services;
using VERA.Infrastructure.Persistence;

namespace VERA.Infrastructure.Services;

public class VerificationService : IVerificationService
{
    private readonly VeraDbContext _context;

    public VerificationService(VeraDbContext context)
    {
        _context = context;
    }

    public async Task CreateVerificationAsync(
        Guid userId,
        CreateVerificationRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.UserId == userId &&
                !u.IsDeleted);

        if (user is null)
        {
            throw new AppException(
                "Kullanıcı bulunamadı.",
                "USER_NOT_FOUND",
                404);
        }

        var verification = await _context.Verifications
            .FirstOrDefaultAsync(v => v.UserId == userId);

        if (verification is not null)
        {
            throw new AppException(
                "Bu kullanıcı için doğrulama kaydı zaten mevcut.",
                "VERIFICATION_ALREADY_EXISTS",
                409);
        }

        var newVerification = new VERA.Domain.Entities.Verification
        {
            VerificationId = Guid.NewGuid(),
            UserId = userId,
            IdentityVerified = false,
            PhoneVerified = false,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _context.Verifications.Add(newVerification);

        await _context.SaveChangesAsync();
    }

    public async Task<object> GetStatusAsync(Guid userId)
    {
        var verification = await _context.Verifications
            .FirstOrDefaultAsync(v =>
                v.UserId == userId);

        if (verification is null)
        {
            throw new AppException(
                "Doğrulama kaydı bulunamadı.",
                "VERIFICATION_NOT_FOUND",
                404);
        }

        return new
        {
            verification.VerificationId,
            verification.IdentityVerified,
            verification.PhoneVerified,
            verification.Status,
            verification.RejectionReason,
            verification.VerifiedAt
        };
    }
}