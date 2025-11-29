using Shared.Dtos.IdentityModule;

namespace Presentation.Controllers
{
    public class AuthenticationController(IServiceManager _serviceManager) : ApiController
    {
        //Post: api/authentication/Register
        [HttpPost("Register")]
        public async Task<ActionResult<UserResultDto>> RegisterAsync(RegisterDto registerDto)
            => Ok(await (_serviceManager.AuthenticationService.RegisterAsync(registerDto)));

        //Post : api/authentication/Login
        [HttpPost("Login")]
        public async Task<ActionResult<UserResultDto>> LoginAsync(LoginDto loginDto)
            => Ok(await (_serviceManager.AuthenticationService.LoginAsync(loginDto)));

        //Post : api/authentication/RefreshToken
        [HttpPost("RefreshToken")]
        public async Task<ActionResult<TokenDto>> RefreshToken([FromBody] string refreshToken)
        {
            var ip = _serviceManager.ClientIpProvider.GetClientIp();
            var tokens = await _serviceManager.TokenService.RefreshTokenAsync(refreshToken, ip);
            return Ok(tokens);
        }

        //Post : api/authentication/RevokeToken
        [HttpPost("RevokeToken")]
        public async Task<ActionResult> RevokeToken([FromBody] string refreshToken)
        {
            var ip = _serviceManager.ClientIpProvider.GetClientIp();
            await _serviceManager.TokenService.RevokeRefreshTokenAsync(refreshToken, ip, "Revoked by user");
            return NoContent();
        }
    }
}
