using Domain.Contracts.IdentityDb;
using Domain.Entities.IdentityModule;

namespace Presistance.IdentityDb.IdentityRepositories
{
    public class RefreshTokenRepository(IdentityStoreDbContext _identityStoreDb) : IRefreshTokenRepository

    {
        public async Task AddRefreshTokenAsync(RefreshToken refreshToken)
        => await _identityStoreDb.AddAsync(refreshToken);

        public async Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash)
        => await _identityStoreDb.RefreshTokens.FirstOrDefaultAsync(s => s.TokenHash == tokenHash); 
            
        public async Task SaveChangesAsync()
        => await _identityStoreDb.SaveChangesAsync();
    }
}
