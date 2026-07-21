using Microsoft.Extensions.Options;
using Ft.Consultorio.ServiceDefaults.Infrastructure.Settings;

namespace Ft.Consultorio.ServiceDefaults.Infrastructure.Http
{
    public sealed class InternalApiKeyHandler : DelegatingHandler
    {
        private const string HeaderName = "X-Internal-Api-Key";
        private readonly string _apiKey;

        public InternalApiKeyHandler(IOptions<InternalApiKeyOptions> options)
        {
            _apiKey = options.Value.Key;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(_apiKey))
            {
                request.Headers.Remove(HeaderName);
                request.Headers.Add(HeaderName, _apiKey);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
