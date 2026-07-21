using Microsoft.AspNetCore.Http;

namespace Ft.Consultorio.ServiceDefaults.Web.Api.RateLimiting
{
    public static class RateLimitPartitionKeyFactory
    {
        public static string ForIpAndPath(HttpContext httpContext, string policyName)
        {
            var ip = GetClientIp(httpContext);
            var path = NormalizePath(httpContext.Request.Path.Value);
            return $"{policyName}:{ip}:{path}";
        }

        public static string ForUserOrIpAndPath(HttpContext httpContext, string policyName)
        {
            var userId = httpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
                ?? httpContext.User.FindFirst("sub")?.Value;

            var subject = string.IsNullOrWhiteSpace(userId)
                ? $"ip:{GetClientIp(httpContext)}"
                : $"user:{userId}";

            var path = NormalizePath(httpContext.Request.Path.Value);
            return $"{policyName}:{subject}:{path}";
        }

        private static string GetClientIp(HttpContext httpContext)
        {
            // RemoteIpAddress ya refleja la IP real del cliente cuando la petición
            // atraviesa un proxy confiable: UseForwardedHeaders la sobreescribe solo
            // si el salto viene de KnownIPNetworks. Leer X-Forwarded-For a mano aquí
            // permitiría falsificar la partición del rate limiter.
            return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private static string NormalizePath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return "/";
            }

            return path.Trim().ToLowerInvariant();
        }
    }
}
