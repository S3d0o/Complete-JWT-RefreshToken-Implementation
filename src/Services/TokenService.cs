using Domain.Contracts.IdentityDb;
using Domain.Entities.IdentityModule;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Shared.Dtos.IdentityModule;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Services.Implementations
{
    public class TokenServices : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly UserManager<User> _userManager;
        private readonly IIdentityUnitOfWork _db;

        private readonly int AccessTokenExpirationMinutes = 10;
        private readonly int RefreshTokenExpirationDays = 7;
        private readonly int TheftDetectionWindowMinutes = 5;

        public TokenServices(IConfiguration config, UserManager<User> userManager, IIdentityUnitOfWork db)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<TokenDto> GenerateTokensAsync(User user, IList<string> roles, string ipAddress)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            roles ??= new List<string>();

            // Generate access token
            var accessToken = GenerateAccessToken(user, roles);

            // Snapshot security stamp for invalidation
            var securityStamp = await _userManager.GetSecurityStampAsync(user);

            // Refresh token generation + hashing
            var refreshToken = GenerateRefreshToken();
            var refreshTokenHash = HashRefreshToken(refreshToken);
            var refreshTokenEntity = CreateRefreshEntity(refreshTokenHash, user.Id, ipAddress, RefreshTokenExpirationDays, securityStamp);

            // Persist refresh token
            await _db.RefreshTokenRepository.AddRefreshTokenAsync(refreshTokenEntity);
            await _db.SaveChangesAsync();

            var expiresAt = DateTime.UtcNow.AddMinutes(AccessTokenExpirationMinutes);
            return new TokenDto(accessToken, refreshToken, expiresAt);
        }

        public async Task<TokenDto> RefreshTokenAsync(string refreshToken, string ipAddress)
        {
            var tokenHash = HashRefreshToken(refreshToken);

            using var transaction = await _db.BeginTransactionAsync();
            var existingToken = await _db.RefreshTokenRepository.GetRefreshTokenByHashAsync(tokenHash);

            if (existingToken == null)
                throw new SecurityTokenException("Invalid refresh token");

            if (!existingToken.IsActive)
                throw new SecurityTokenException("Inactive refresh token");

            if (existingToken.Revoked != null)
                throw new SecurityTokenException("Refresh token already revoked");

            if (existingToken.IsExpired)
                throw new SecurityTokenException("Refresh token expired");

            if (!string.IsNullOrEmpty(existingToken.ReplacedByTokenHash))
                throw new SecurityTokenException("Refresh token already rotated");

            // Theft detection
            if (existingToken.LastUsed != null)
            {
                var minutes = (DateTime.UtcNow - existingToken.LastUsed.Value).TotalMinutes;
                if (minutes < TheftDetectionWindowMinutes && existingToken.LastUsedByIp != ipAddress)
                    throw new SecurityTokenException("Suspicious refresh token activity detected");
            }

            // Load user for security-stamp check
            var user = await _userManager.FindByIdAsync(existingToken.UserId)
                ?? throw new SecurityTokenException("Invalid token - User not found");

            var currentStamp = await _userManager.GetSecurityStampAsync(user);
            if (existingToken.SecurityStamp != currentStamp)
                throw new SecurityTokenException("Refresh token invalidated due to user security changes");

            // Mark existing token as rotated
            existingToken.LastUsed = DateTime.UtcNow;
            existingToken.LastUsedByIp = ipAddress;
            existingToken.RevocationReason = "Rotated";
            existingToken.Revoked = DateTime.UtcNow;
            existingToken.RevokedByIp = ipAddress;

            // Generate new refresh token
            var newRefreshToken = GenerateRefreshToken();
            var newRefreshTokenHash = HashRefreshToken(newRefreshToken);
            existingToken.ReplacedByTokenHash = newRefreshTokenHash;

            var newTokenEntity = CreateRefreshEntity(
                newRefreshTokenHash,
                user.Id,
                ipAddress,
                RefreshTokenExpirationDays,
                currentStamp
            );

            await _db.RefreshTokenRepository.AddRefreshTokenAsync(newTokenEntity);

            try
            {
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                throw new SecurityTokenException("Refresh token already used");
            }

            // New access token
            var roles = await _userManager.GetRolesAsync(user);
            return new TokenDto(
                AccessToken: GenerateAccessToken(user, roles),
                RefreshToken: newRefreshToken,
                AccessTokenExpiresAt: DateTime.UtcNow.AddMinutes(AccessTokenExpirationMinutes)
            );
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken, string ipAddress, string? reason = null)
        {
            var tokenHash = HashRefreshToken(refreshToken);
            var existingToken = await _db.RefreshTokenRepository.GetRefreshTokenByHashAsync(tokenHash);

            if (existingToken == null)
                throw new KeyNotFoundException("Refresh token not found");

            if (existingToken.Revoked == null)
            {
                existingToken.Revoked = DateTime.UtcNow;
                existingToken.RevokedByIp = ipAddress;
                existingToken.RevocationReason = reason ?? "RevokedByUser";
                await _db.SaveChangesAsync();
            }
        }

        #region Helpers

        private string GenerateAccessToken(User user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.DisplayName ?? user.UserName ?? "")
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddMinutes(AccessTokenExpirationMinutes);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken() =>
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        private string HashRefreshToken(string raw) =>
            Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));

        private RefreshToken CreateRefreshEntity(string tokenHash, string userId, string ip, int days, string? securityStamp) =>
            new RefreshToken
            {
                TokenHash = tokenHash,
                UserId = userId,
                Expires = DateTime.UtcNow.AddDays(days),
                Created = DateTime.UtcNow,
                CreatedByIp = ip,
                SecurityStamp = securityStamp
            };

        #endregion
    }
}
