using AutoMapper;
using Domain.Contracts.IdentityDb;
using Domain.Contracts.StoreDb;
using Domain.Entities.IdentityModule;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using static Shared.HelperClasses.GetIpHelper;

namespace Services.Implementations
{
    public class ServiceManager(
          UserManager<User> _userManager
        , ITokenService _tokenService
        , IClientIpProvider _clientIpProvider
        , IConfiguration _config
        , UserManager<User> _user
        , IIdentityUnitOfWork _db
        , IHttpContextAccessor _contextAccessor) : IServiceManager
    {
        private readonly Lazy<IAuthenticationService> _authenticationService = new Lazy<IAuthenticationService>(() => new AuthenticationService(_userManager, _tokenService, _clientIpProvider));
        private readonly Lazy<ITokenService> _tokenService = new Lazy<ITokenService>(() => new TokenServices(_config,_user,_db));
        private readonly Lazy<IClientIpProvider> _clientIp = new Lazy<IClientIpProvider>(() => new ClientIpProvider(_contextAccessor));

        public IAuthenticationService AuthenticationService => _authenticationService.Value;
        public ITokenService TokenService => _tokenService.Value;
        public IClientIpProvider ClientIpProvider => _clientIp.Value;
    }
}
