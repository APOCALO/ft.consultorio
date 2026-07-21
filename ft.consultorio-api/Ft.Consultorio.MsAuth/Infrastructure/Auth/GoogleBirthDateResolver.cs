using System.Net.Http.Headers;
using System.Text.Json;
using Ft.Consultorio.MsAuth.Application.Interfaces.Auth;

namespace Ft.Consultorio.MsAuth.Infrastructure.Auth
{
    public sealed class GoogleBirthDateResolver : IGoogleBirthDateResolver
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private readonly HttpClient _httpClient;
        private readonly ILogger<GoogleBirthDateResolver> _logger;

        public GoogleBirthDateResolver(HttpClient httpClient, ILogger<GoogleBirthDateResolver> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<DateOnly?> TryResolveAsync(string? accessToken, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return null;
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, "people/me?personFields=birthdays");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.SendAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to call Google People API for birthdays.");
                return null;
            }

            using (response)
            {
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogInformation(
                        "Google People API birthdays request failed with status {StatusCode}.",
                        (int)response.StatusCode);
                    return null;
                }

                await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);

                GooglePeopleResponse? payload;
                try
                {
                    payload = await JsonSerializer.DeserializeAsync<GooglePeopleResponse>(
                        contentStream,
                        JsonOptions,
                        cancellationToken);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Invalid JSON from Google People API birthdays response.");
                    return null;
                }

                if (payload?.Birthdays is null || payload.Birthdays.Count == 0)
                {
                    return null;
                }

                DateOnly? fallbackWithYear = null;

                foreach (var birthday in payload.Birthdays)
                {
                    var date = birthday?.Date;
                    if (date is null || date.Year is null || date.Month is null || date.Day is null)
                    {
                        continue;
                    }

                    if (!DateOnly.TryParse($"{date.Year:D4}-{date.Month:D2}-{date.Day:D2}", out var parsed))
                    {
                        continue;
                    }

                    if (birthday?.Metadata?.Primary == true)
                    {
                        return parsed;
                    }

                    fallbackWithYear ??= parsed;
                }

                return fallbackWithYear;
            }
        }

        private sealed record GooglePeopleResponse(List<GoogleBirthday>? Birthdays);

        private sealed record GoogleBirthday(GoogleDate? Date, GoogleMetadata? Metadata);

        private sealed record GoogleMetadata(bool? Primary);

        private sealed record GoogleDate(int? Year, int? Month, int? Day);
    }
}
