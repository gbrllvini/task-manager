using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TaskManager.Application.Auth;
using TaskManager.Application.Services.Interfaces;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Identity;

namespace TaskManager.Application.Services;

public class RefreshTokenService : IRefreshTokenService {
    private readonly AppDbContext _dbContext;
    private readonly JwtOptions _jwtOptions;

    public RefreshTokenService(AppDbContext dbContext, IOptions<JwtOptions> jwtOptions) {
        _dbContext = dbContext;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<RefreshTokenIssueResult> IssueAsync(Guid userId, string? ipAddress, CancellationToken cancellationToken = default) {
        var issuedToken = GenerateToken();

        var entity = new RefreshToken {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = issuedToken.TokenHash,
            ExpiresAtUtc = issuedToken.ExpiresAtUtc,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedByIp = TruncateIp(ipAddress)
        };

        await _dbContext.RefreshTokens.AddAsync(entity, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return issuedToken;
    }

    public async Task<(Guid UserId, RefreshTokenIssueResult IssuedToken)> RotateAsync(string rawToken, string? ipAddress, CancellationToken cancellationToken = default) {
        var tokenHash = ComputeSha256(rawToken);
        var entity = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (entity is null) {
            throw new UnauthorizedAccessException("Refresh token inválido.");
        }

        if (entity.RevokedAtUtc.HasValue || entity.ExpiresAtUtc <= DateTime.UtcNow) {
            throw new UnauthorizedAccessException("Refresh token expirado ou revogado.");
        }

        var issuedToken = GenerateToken();
        entity.RevokedAtUtc = DateTime.UtcNow;
        entity.RevokedByIp = TruncateIp(ipAddress);
        entity.ReplacedByTokenHash = issuedToken.TokenHash;

        var newEntity = new RefreshToken {
            Id = Guid.NewGuid(),
            UserId = entity.UserId,
            TokenHash = issuedToken.TokenHash,
            ExpiresAtUtc = issuedToken.ExpiresAtUtc,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedByIp = TruncateIp(ipAddress)
        };

        await _dbContext.RefreshTokens.AddAsync(newEntity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return (entity.UserId, issuedToken);
    }

    public async Task RevokeAsync(Guid userId, string rawToken, string? ipAddress, CancellationToken cancellationToken = default) {
        var tokenHash = ComputeSha256(rawToken);
        var entity = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.UserId == userId && rt.TokenHash == tokenHash, cancellationToken);

        if (entity is null || entity.RevokedAtUtc.HasValue) {
            return;
        }

        entity.RevokedAtUtc = DateTime.UtcNow;
        entity.RevokedByIp = TruncateIp(ipAddress);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeCurrentAsync(string rawToken, string? ipAddress, CancellationToken cancellationToken = default) {
        var tokenHash = ComputeSha256(rawToken);
        var entity = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (entity is null || entity.RevokedAtUtc.HasValue) {
            return;
        }

        entity.RevokedAtUtc = DateTime.UtcNow;
        entity.RevokedByIp = TruncateIp(ipAddress);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public int GetRefreshTokenDays() {
        return _jwtOptions.RefreshTokenDays;
    }

    private RefreshTokenIssueResult GenerateToken() {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        var rawToken = Convert.ToBase64String(randomBytes);
        var expiresAtUtc = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays);

        return new RefreshTokenIssueResult {
            RawToken = rawToken,
            TokenHash = ComputeSha256(rawToken),
            ExpiresAtUtc = expiresAtUtc
        };
    }

    private static string ComputeSha256(string value) {
        var bytes = Encoding.UTF8.GetBytes(value);
        var hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash);
    }

    private static string? TruncateIp(string? ipAddress) {
        if (string.IsNullOrWhiteSpace(ipAddress)) {
            return null;
        }

        return ipAddress.Length <= 64 ? ipAddress : ipAddress[..64];
    }
}
