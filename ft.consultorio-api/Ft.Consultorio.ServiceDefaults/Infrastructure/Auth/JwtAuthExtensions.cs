using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Ft.Consultorio.ServiceDefaults.Infrastructure.Settings;

namespace Ft.Consultorio.ServiceDefaults.Infrastructure.Auth
{
    public static class JwtAuthExtensions
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>(configuration.GetSection("ServiceDefaults:JwtSettings"));

            var jwtSettings = configuration.GetSection("ServiceDefaults:JwtSettings").Get<JwtSettings>()
                ?? throw new InvalidOperationException("JwtSettings configuration section is missing or invalid.");

            var (signingKey, validAlgorithms) = BuildValidationKey(jwtSettings);

            var validIssuers = BuildValidValues(jwtSettings.Issuer, jwtSettings.AdditionalIssuers);
            var validAudiences = BuildValidValues(jwtSettings.Audience, jwtSettings.AdditionalAudiences);

            if (validIssuers.Length == 0 || validAudiences.Length == 0)
            {
                throw new InvalidOperationException("JwtSettings must define at least one issuer and one audience.");
            }

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "FtConsultorioJwt";
                options.DefaultChallengeScheme = "FtConsultorioJwt";
            })
            .AddJwtBearer("FtConsultorioJwt", options =>
            {
                options.RequireHttpsMetadata = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuers = validIssuers,
                    ValidateIssuer = true,
                    ValidAudiences = validAudiences,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    // Restringe los algoritmos aceptados: evita que un token HS256 firmado
                    // con material conocido sea aceptado cuando la validación es RSA.
                    ValidAlgorithms = validAlgorithms,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(2),
                    NameClaimType = JwtRegisteredClaimNames.Sub,
                    RoleClaimType = ClaimTypes.Role
                };

                // SignalR (WebSocket) no puede enviar el header Authorization en el handshake:
                // manda el token por query (?access_token=...). Lo leemos solo para rutas de hub.
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization();
            return services;
        }

        private static (SecurityKey Key, string[] Algorithms) BuildValidationKey(JwtSettings settings)
        {
            // Preferido: validación asimétrica RS256. Los servicios solo tienen la
            // clave pública; únicamente el emisor (MsAuth) puede firmar tokens.
            if (!string.IsNullOrWhiteSpace(settings.PublicKeyPem))
            {
                var rsa = System.Security.Cryptography.RSA.Create();
                try
                {
                    rsa.ImportFromPem(settings.PublicKeyPem);
                }
                catch (ArgumentException ex)
                {
                    throw new InvalidOperationException(
                        "JwtSettings.PublicKeyPem must be a valid PEM-encoded RSA public key.", ex);
                }

                return (new RsaSecurityKey(rsa), [SecurityAlgorithms.RsaSha256]);
            }

            // Legado/pruebas: clave simétrica HS256.
            if (string.IsNullOrWhiteSpace(settings.SecretKey))
            {
                throw new InvalidOperationException(
                    "JwtSettings requires PublicKeyPem (preferred, RS256) or SecretKey (legacy, HS256).");
            }

            byte[] keyBytes;
            try
            {
                keyBytes = Convert.FromBase64String(settings.SecretKey);
            }
            catch (FormatException)
            {
                // Las claves de prueba no siempre son Base64; usa los bytes UTF-8.
                keyBytes = System.Text.Encoding.UTF8.GetBytes(settings.SecretKey);
            }

            if (keyBytes.Length < 32)
            {
                throw new InvalidOperationException(
                    $"JwtSettings.SecretKey must decode to at least 32 bytes (256 bits) for HS256. " +
                    $"Current key decodes to {keyBytes.Length} bytes.");
            }

            return (new SymmetricSecurityKey(keyBytes), [SecurityAlgorithms.HmacSha256]);
        }

        private static string[] BuildValidValues(string primary, IReadOnlyCollection<string>? additional)
        {
            var values = new List<string>();
            AddValues(values, primary);

            if (additional is not null)
            {
                foreach (var value in additional)
                {
                    AddValues(values, value);
                }
            }

            return values
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private static void AddValues(List<string> destination, string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return;
            }

            foreach (var item in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                destination.Add(item);
            }
        }
    }
}
