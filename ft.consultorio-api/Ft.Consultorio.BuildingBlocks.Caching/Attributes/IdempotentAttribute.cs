using Ft.Consultorio.ServiceDefaults.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ft.Consultorio.ServiceDefaults.Web.Api.Extensions;

namespace Ft.Consultorio.ServiceDefaults.Web.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class IdempotentAttribute : Attribute, IAsyncActionFilter
    {
        private const int DefaultCacheTimeInMinutes = 15;
        private readonly TimeSpan _cacheDuration;

        // Si quisieras forzar GUID, ponlo en true.
        private const bool RequireGuid = false;

        public IdempotentAttribute(int cacheTimeInMinutes = DefaultCacheTimeInMinutes)
        {
            _cacheDuration = TimeSpan.FromMinutes(cacheTimeInMinutes);
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var headers = context.HttpContext.Request.Headers;

            // Acepta ambos nombres de header.
            bool hasKey = headers.TryGetValue("Idempotence-Key", out StringValues keyValues) || headers.TryGetValue("Idempotency-Key", out keyValues);

            if (!hasKey)
            {
                context.Result = new BadRequestObjectResult("Invalid or missing Idempotence-Key / Idempotency-Key header");
                return;
            }

            string keyRaw = keyValues.ToString().Trim();
            if (string.IsNullOrWhiteSpace(keyRaw))
            {
                context.Result = new BadRequestObjectResult("Idempotence key is empty");
                return;
            }

            if (RequireGuid && !Guid.TryParse(keyRaw, out _))
            {
                context.Result = new BadRequestObjectResult("Idempotence key must be a GUID");
                return;
            }

            // Normaliza la clave a un hash, aislando por usuario y path para evitar colisiones cruzadas.
            var userId = context.HttpContext.User.GetUserId()?.ToString() ?? "anon";
            var path = context.HttpContext.Request.Path.Value ?? string.Empty;
            string cacheKey = "Idem:" + ComputeSha256($"{userId}|{path}|{keyRaw}");

            var cache = context.HttpContext.RequestServices.GetRequiredService<IRedisCacheService>();

            // ¿Ya existe respuesta cacheada?
            var cached = await cache.GetAsync<CachedPayload>(cacheKey);
            if (cached is not null)
            {
                context.Result = new ContentResult
                {
                    StatusCode = cached.StatusCode,
                    ContentType = "application/json",
                    Content = cached.JsonBody
                };
                return;
            }

            // Ejecuta la acción.
            var executed = await next();

            // Cachea solo 2xx.
            if (executed.Result is ObjectResult { StatusCode: >= 200 and < 300 } ok)
            {
                int status = ok.StatusCode ?? StatusCodes.Status200OK;

                string bodyJson = ok.Value is string s
                    ? s
                    : JsonSerializer.Serialize(ok.Value);

                var payload = new CachedPayload(status, bodyJson);

                await cache.SetAsync(cacheKey, payload, _cacheDuration);
            }
        }

        private static string ComputeSha256(string raw)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(raw);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash); // 64 chars
        }

        private sealed record CachedPayload(int StatusCode, string JsonBody);
    }
}
