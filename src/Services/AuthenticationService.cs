using Domain.Entities.IdentityModule;
using Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Shared.Dtos.IdentityModule;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static Shared.HelperClasses.GetIpHelper;

namespace Services.Implementations
{
    public class AuthenticationService(UserManager<User> _userManager
        , ITokenService _tokenService, IClientIpProvider _clientIpProvider) : IAuthenticationService
    {

        public async Task<UserResultDto> LoginAsync(LoginDto loginDto)
        {
            
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
                
            if (user == null) 
                throw new UnAuthorizedException();

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!isPasswordValid)
                throw new UnAuthorizedException();

            string ip = _clientIpProvider.GetClientIp();
            var roles = await _userManager.GetRolesAsync(user);
            var tokens = await _tokenService.GenerateTokensAsync(user, roles, ip);

            return new UserResultDto(user.DisplayName, tokens.AccessToken, user.Email!);
        }

        public async Task<UserResultDto> RegisterAsync(RegisterDto registerDto)
        {
            var user = new User
            {
                Email = registerDto.Email,
                UserName = registerDto.UserName,
                DisplayName = registerDto.DisplayName,
                PhoneNumber = registerDto.PhoneNumber
            };
            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                throw new ValidationException(errors);
            }

            // optionally assign default role
            //await _userManager.AddToRoleAsync(user, "User");

            var roles = await _userManager.GetRolesAsync(user);
            var ip = _clientIpProvider.GetClientIp();
            var tokens = await _tokenService.GenerateTokensAsync(user, roles, ip);

            return new UserResultDto(user.DisplayName, tokens.AccessToken, user.Email!);
        }
    }
}
