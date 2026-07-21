using System.Security.Claims;

namespace Ft.Consultorio.ServiceDefaults.Web.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Obtiene el UserId (Guid) desde los claims del token.
        /// Busca primero en NameIdentifier, luego en "sub".
        /// </summary>
        /// <param name="user">Claims principal del usuario autenticado.</param>
        /// <returns>Guid del usuario o null si no existe/parsea.</returns>
        public static Guid? GetUserId(this ClaimsPrincipal user)
        {
            if (user == null) return null;

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? user.FindFirst("sub")?.Value;

            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            return null;
        }
    }
}
