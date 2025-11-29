using Microsoft.AspNetCore.Http;

namespace Shared.HelperClasses
{
    public class GetIpHelper
    {
        public interface IClientIpProvider
        {
            string GetClientIp();
        }

        public class ClientIpProvider(IHttpContextAccessor _httpContextAccessor) : IClientIpProvider
        {
            public string GetClientIp()
            {
                var context = _httpContextAccessor.HttpContext;
                if (context == null)
                    return "unknown";

                // Check X-Forwarded-For (client IP when behind proxy)
                var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(forwardedFor))
                {
                    // Can contain multiple IPs: client, proxy1, proxy2...
                    var firstIp = forwardedFor.Split(',').First().Trim();
                    return firstIp;
                }

                // Fallback to direct connection IP
                return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            }
        }
    }
}
