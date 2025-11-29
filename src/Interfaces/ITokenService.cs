using Domain.Entities.IdentityModule;
using Shared.Dtos.IdentityModule;

namespace Service.Abstraction.Contracts
{
    public interface ITokenService
    {
        Task<TokenDto> GenerateTokensAsync(User user, IList<string> roles, string ipAddress);
        Task<TokenDto> RefreshTokenAsync(string refreshToken, string ipAddress);
        Task RevokeRefreshTokenAsync(string refreshToken, string ipAddress, string? reason = null);
    }
}
