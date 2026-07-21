using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;

namespace Ft.Consultorio.ServiceDefaults.Infrastructure.Http
{
    public interface IHttpApiClient
    {
        Task<T> SendAsync<T>(
            HttpClient client,
            HttpRequestMessage request,
            string serviceName,
            CancellationToken cancellationToken);

        Task<T> ReadAsync<T>(
            HttpResponseMessage response,
            HttpRequestMessage request,
            string serviceName,
            CancellationToken cancellationToken);
    }

    public sealed class HttpApiClient : IHttpApiClient
    {
        private readonly Microsoft.AspNetCore.Mvc.JsonOptions _jsonOptions;
        private readonly ILogger<HttpApiClient> _logger;

        public HttpApiClient(
            IOptions<Microsoft.AspNetCore.Mvc.JsonOptions> jsonOptions,
            ILogger<HttpApiClient> logger)
        {
            _jsonOptions = jsonOptions?.Value ?? throw new ArgumentNullException(nameof(jsonOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<T> SendAsync<T>(
            HttpClient client,
            HttpRequestMessage request,
            string serviceName,
            CancellationToken cancellationToken)
        {
            HttpResponseMessage response;
            try
            {
                response = await client.SendAsync(request, cancellationToken);
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "{Service} request timeout for {Method} {Url}.",
                    serviceName,
                    request.Method.Method,
                    request.RequestUri);
                throw new HttpRequestException(
                    message: $"{serviceName} request timed out.",
                    inner: ex,
                    statusCode: HttpStatusCode.GatewayTimeout);
            }

            try
            {
                return await ReadAsync<T>(response, request, serviceName, cancellationToken);
            }
            finally
            {
                response.Dispose();
            }
        }

        public async Task<T> ReadAsync<T>(
            HttpResponseMessage response,
            HttpRequestMessage request,
            string serviceName,
            CancellationToken cancellationToken)
        {
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("{Service} responded {StatusCode} for {Method} {Url}.",
                    serviceName,
                    (int)response.StatusCode,
                    request.Method.Method,
                    request.RequestUri);
                throw new ExternalServiceException(serviceName, response.StatusCode, body);
            }

            if (typeof(T) == typeof(string))
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return (T)(object)content;
            }

            var payload = await response.Content.ReadFromJsonAsync<T>(_jsonOptions.JsonSerializerOptions, cancellationToken);
            if (payload is null)
            {
                _logger.LogWarning("{Service} returned empty payload for {Method} {Url}",
                    serviceName,
                    request.Method.Method,
                    request.RequestUri);
                throw new ExternalServiceException(serviceName, response.StatusCode, "Empty payload");
            }

            return payload;
        }
    }
}
