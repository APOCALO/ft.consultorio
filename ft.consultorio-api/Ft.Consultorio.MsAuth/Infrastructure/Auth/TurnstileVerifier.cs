using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Ft.Consultorio.MsAuth.Application.Interfaces.Auth;
using Ft.Consultorio.MsAuth.Infrastructure.Settings;

namespace Ft.Consultorio.MsAuth.Infrastructure.Auth
{
    public sealed class TurnstileVerifier : ITurnstileVerifier
    {
        private readonly HttpClient _httpClient;
        private readonly TurnstileSettings _settings;
        private readonly ILogger<TurnstileVerifier> _logger;

        public TurnstileVerifier(
            HttpClient httpClient,
            IOptions<TurnstileSettings> options,
            ILogger<TurnstileVerifier> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> VerifyAsync(string? token, string? remoteIp, CancellationToken cancellationToken)
        {
            // Degradación elegante: si está deshabilitado, no se exige captcha.
            if (!_settings.Enabled)
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(_settings.SecretKey))
            {
                // Mal configurado: fail closed para no dejar el endpoint sin protección.
                _logger.LogError("Turnstile está habilitado pero falta la SecretKey. Se rechaza la solicitud.");
                return false;
            }

            // Token ausente: rechazar sin contactar a Cloudflare.
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            try
            {
                var fields = new Dictionary<string, string>
                {
                    ["secret"] = _settings.SecretKey,
                    ["response"] = token,
                };

                if (!string.IsNullOrWhiteSpace(remoteIp))
                {
                    fields["remoteip"] = remoteIp;
                }

                using var content = new FormUrlEncodedContent(fields);
                using var response = await _httpClient.PostAsync(_settings.VerifyEndpoint, content, cancellationToken);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<TurnstileVerifyResponse>(cancellationToken);

                if (result is null)
                {
                    _logger.LogWarning("Respuesta vacía de Turnstile siteverify.");
                    return false;
                }

                if (!result.Success)
                {
                    _logger.LogWarning(
                        "Verificación de Turnstile fallida. error-codes: {ErrorCodes}",
                        string.Join(", ", result.ErrorCodes ?? Array.Empty<string>()));
                }

                return result.Success;
            }
            catch (Exception ex)
            {
                // Fail closed: cualquier error de red/parsing rechaza la solicitud.
                _logger.LogError(ex, "Error al verificar el token de Turnstile.");
                return false;
            }
        }

        private sealed record TurnstileVerifyResponse
        {
            [JsonPropertyName("success")]
            public bool Success { get; init; }

            [JsonPropertyName("error-codes")]
            public string[]? ErrorCodes { get; init; }
        }
    }
}
