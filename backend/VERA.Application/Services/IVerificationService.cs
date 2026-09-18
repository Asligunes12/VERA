using VERA.Application.DTOs.Verification;

namespace VERA.Application.Services;

public interface IVerificationService
{
    Task CreateVerificationAsync(
        Guid userId,
        CreateVerificationRequest request);

    Task<object> GetStatusAsync(Guid userId);
}