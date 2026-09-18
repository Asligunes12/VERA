using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VERA.Application.DTOs.Auth;
using VERA.Application.Services;
using VERA.Domain.Entities;
using VERA.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using VERA.Application.Exceptions;

namespace VERA.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly VeraDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly PasswordHasher<User> _passwordHasher;

    public AuthService(
        VeraDbContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Email == email &&
                !u.IsDeleted);

        if (existingUser is not null)
            throw new AppException(
            "Bu e-posta adresi zaten kayıtlı.",
            "EMAIL_ALREADY_EXISTS",
             409);

        var user = new User
        {
            UserId = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            PhoneNumber = request.PhoneNumber?.Trim(),
            Role = "User",
            IsActive = true,
            IsDeleted = false,
            TrustScore = 0,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(
            user,
            request.Password);

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return await CreateAuthResponseAsync(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Email == email &&
                !u.IsDeleted);

        if (user is null)
    {
        throw new AppException(
         "E-posta veya şifre hatalı.",
         "INVALID_CREDENTIALS",
          401);
    }

       if (!user.IsActive)
    {
        throw new AppException(
         "Kullanıcı hesabı aktif değil.",
         "INACTIVE_USER",
         401);
    }

        var passwordResult = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (passwordResult == PasswordVerificationResult.Failed)
            throw new AppException(
        "E-posta veya şifre hatalı.",
        "INVALID_CREDENTIALS",
        401);

        return await CreateAuthResponseAsync(user);
    }
    public async Task<AuthResponse> RefreshAsync(string refreshToken)
{
         if (string.IsNullOrWhiteSpace(refreshToken))
            throw new AppException(
              "Refresh token gerekli.",
              "REFRESH_TOKEN_REQUIRED",
                400);

    var tokenHash = HashRefreshToken(refreshToken);

    var storedToken = await _context.RefreshTokens
        .Include(r => r.User)
        .FirstOrDefaultAsync(r =>
            r.TokenHash == tokenHash &&
            !r.IsDeleted);

    if (storedToken is null)
    throw new AppException(
        "Geçersiz refresh token.",
        "INVALID_REFRESH_TOKEN",
        401);

    if (storedToken.RevokedAt.HasValue)
    throw new AppException(
        "Refresh token artık geçerli değil.",
        "REVOKED_REFRESH_TOKEN",
        401);

    if (storedToken.ExpiresAt <= DateTime.UtcNow)
    throw new AppException(
        "Refresh token'ın süresi dolmuş.",
        "EXPIRED_REFRESH_TOKEN",
        401);

    var user = storedToken.User;

    if (user.IsDeleted || !user.IsActive)
    throw new AppException(
        "Kullanıcı hesabı aktif değil.",
        "INACTIVE_USER",
        401);

    var newAccessToken = GenerateJwtToken(user);
    var newRefreshToken = GenerateRefreshToken();

    storedToken.RevokedAt = DateTime.UtcNow;
    storedToken.ReplacedByTokenHash =
        HashRefreshToken(newRefreshToken);

    var newRefreshTokenEntity = new RefreshToken
    {
        RefreshTokenId = Guid.NewGuid(),
        UserId = user.UserId,
        TokenHash = HashRefreshToken(newRefreshToken),
        ExpiresAt = DateTime.UtcNow.AddDays(
            _configuration.GetValue<int>(
                "RefreshToken:ExpireDays")),
        CreatedAt = DateTime.UtcNow
    };

    _context.RefreshTokens.Add(newRefreshTokenEntity);

    await _context.SaveChangesAsync();

    return new AuthResponse
    {
        Token = newAccessToken,
        RefreshToken = newRefreshToken,
        UserId = user.UserId,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email
    };
}
   public async Task LogoutAsync(string refreshToken)
{
    if (string.IsNullOrWhiteSpace(refreshToken))
    throw new AppException(
        "Refresh token gerekli.",
        "REFRESH_TOKEN_REQUIRED",
        400);
    var tokenHash = HashRefreshToken(refreshToken);

    var storedToken = await _context.RefreshTokens
        .FirstOrDefaultAsync(r =>
            r.TokenHash == tokenHash &&
            !r.IsDeleted);

    if (storedToken is null)
    throw new AppException(
        "Geçersiz refresh token.",
        "INVALID_REFRESH_TOKEN",
        401);

    if (!storedToken.RevokedAt.HasValue)
    {
        storedToken.RevokedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
}

    private async Task<AuthResponse> CreateAuthResponseAsync(User user)
   {
    var token = GenerateJwtToken(user);
    var refreshToken = await CreateAndStoreRefreshTokenAsync(user);

    return new AuthResponse
    {
        Token = token,
        RefreshToken = refreshToken,
        UserId = user.UserId,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email
     };
   }

    private string GenerateJwtToken(User user)
    {
        var key = _configuration["Jwt:Key"];

        if (string.IsNullOrWhiteSpace(key))
    throw new AppException(
        "JWT anahtarı yapılandırılmamış.",
        "JWT_KEY_NOT_CONFIGURED",
        500);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name,
                $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key));

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var expireMinutes = _configuration
            .GetValue<int>("Jwt:ExpireMinutes");

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
      private string GenerateRefreshToken()
{
    var randomBytes = RandomNumberGenerator.GetBytes(64);

    return Convert.ToBase64String(randomBytes);
}

private static string HashRefreshToken(string refreshToken)
{
        var hash = SHA256.HashData(
        Encoding.UTF8.GetBytes(refreshToken));

    return Convert.ToHexString(hash);
    }
    private async Task<string> CreateAndStoreRefreshTokenAsync(User user)
{
    var refreshToken = GenerateRefreshToken();

    var refreshTokenEntity = new RefreshToken
    {
        RefreshTokenId = Guid.NewGuid(),
        UserId = user.UserId,
        TokenHash = HashRefreshToken(refreshToken),
        ExpiresAt = DateTime.UtcNow.AddDays(
            _configuration.GetValue<int>("RefreshToken:ExpireDays")),
        CreatedAt = DateTime.UtcNow
    };

    _context.RefreshTokens.Add(refreshTokenEntity);

    await _context.SaveChangesAsync();

    return refreshToken;
 }
}