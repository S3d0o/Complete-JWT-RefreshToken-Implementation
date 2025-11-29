using Domain.Entities.IdentityModule;

namespace Domain.Contracts.IdentityDb
{
    public interface IRefreshTokenRepository
    {
        // Add a new refresh token
        public Task AddRefreshTokenAsync(RefreshToken refreshToken);
        // Get a refresh token by its hash
        public Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash);
        // save changes
        public Task SaveChangesAsync();

    }
}
