using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ft.Consultorio.ServiceDefaults.Infrastructure.Settings;

namespace Ft.Consultorio.ServiceDefaults.Infrastructure.Storage
{
    public sealed class S3BucketInitializerService : BackgroundService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly StorageSettings _settings;
        private readonly ILogger<S3BucketInitializerService> _logger;

        public S3BucketInitializerService(
            IAmazonS3 s3Client,
            IOptions<StorageSettings> options,
            ILogger<S3BucketInitializerService> logger)
        {
            _s3Client = s3Client;
            _settings = options.Value;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_settings.Buckets.Length == 0)
            {
                _logger.LogInformation("No storage buckets configured for initialization.");
                return;
            }

            foreach (var bucketName in _settings.Buckets.Select(b => b.Name).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                try
                {
                    if (await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName))
                        continue;

                    await _s3Client.PutBucketAsync(new PutBucketRequest
                    {
                        BucketName = bucketName
                    }, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error ensuring bucket {Bucket}", bucketName);
                }
            }
        }
    }
}
