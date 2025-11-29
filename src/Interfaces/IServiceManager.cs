using static Shared.HelperClasses.GetIpHelper;

namespace Service.Abstraction.Contracts
{
    public interface IServiceManager
    {
        public IAuthenticationService AuthenticationService { get; }
        public ITokenService TokenService { get; }
        public IClientIpProvider ClientIpProvider { get; }
    }
}
