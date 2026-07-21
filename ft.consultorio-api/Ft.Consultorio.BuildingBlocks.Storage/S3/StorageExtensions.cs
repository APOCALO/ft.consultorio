using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ft.Consultorio.ServiceDefaults.Application.Interfaces;
using Ft.Consultorio.ServiceDefaults.Infrastructure.Settings;

namespace Ft.Consultorio.ServiceDefaults.Infrastructure.Storage
{
    public static class StorageExtensions
    {
        public static IHostApplicationBuilder AddS3Storage(
            this IHostApplicationBuilder builder)
        {
            builder.Services.AddOptions<StorageSettings>()
                .Bind(builder.Configuration.GetSection("ServiceDefaults:StorageSettings"))
                .Validate(ValidateSettings, "Invalid Storage configuration.")
                .ValidateOnStart();

            builder.Services.AddSingleton<IAmazonS3>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<StorageSettings>>().Value;
                var logger = sp.GetRequiredService<ILogger<IAmazonS3>>();

                var config = new AmazonS3Config
                {
                    ServiceURL = settings.ServiceUrl,
                    ForcePathStyle = settings.ForcePathStyle
                };

                if (!string.IsNullOrWhiteSpace(settings.Region))
                {
                    config.AuthenticationRegion = settings.Region;
                }

                logger.LogInformation("Configuring S3 client. ServiceUrl: {ServiceUrl}, ForcePathStyle: {ForcePathStyle}, Region: {Region}",
                    settings.ServiceUrl, settings.ForcePathStyle, settings.Region);

                return new AmazonS3Client(settings.AccessKey, settings.SecretKey, config);
            });

            builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<StorageSettings>>().Value);
            builder.Services.AddSingleton<IFileStorageService, S3FileStorageService>();
            builder.Services.AddSingleton<IPublicAssetUrlResolver, PublicAssetUrlResolver>();
            builder.Services.AddSingleton<IPrivateAssetSigner, PrivateAssetSigner>();
            builder.Services.AddHostedService<S3BucketInitializerService>();

            return builder;
        }

        private static bool ValidateSettings(StorageSettings settings)
        {
            if (settings is null) return false;
            if (string.IsNullOrWhiteSpace(settings.ServiceUrl)) return false;
            if (string.IsNullOrWhiteSpace(settings.AccessKey)) return false;
            if (string.IsNullOrWhiteSpace(settings.SecretKey)) return false;
            return true;
        }
    }
}
