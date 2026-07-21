using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ft.Consultorio.ServiceDefaults.Infrastructure.Settings;
using MvcProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;

namespace Ft.Consultorio.ServiceDefaults.Web.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class InternalOnlyAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private const string HeaderName = "X-Internal-Api-Key";

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var options = context.HttpContext.RequestServices
                .GetRequiredService<IOptions<InternalApiKeyOptions>>();

            var expectedKey = options.Value.Key;

            if (string.IsNullOrEmpty(expectedKey))
            {
                context.Result = new ObjectResult(new MvcProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Internal API Key Error",
                    Detail = "The internal API key is not configured on this service."
                })
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    ContentTypes = { "application/problem+json" }
                };
                return;
            }

            if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var providedKeyValues))
            {
                context.Result = new ObjectResult(new MvcProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Unauthorized",
                    Detail = "Missing X-Internal-Api-Key header."
                })
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    ContentTypes = { "application/problem+json" }
                };
                return;
            }

            var providedKey = providedKeyValues.ToString();

            var expectedBytes = Encoding.UTF8.GetBytes(expectedKey);
            var providedBytes = Encoding.UTF8.GetBytes(providedKey);

            if (!CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes))
            {
                context.Result = new ObjectResult(new MvcProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Unauthorized",
                    Detail = "Invalid internal API key."
                })
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    ContentTypes = { "application/problem+json" }
                };
                return;
            }

            await Task.CompletedTask;
        }
    }
}
