using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using ErrorOr;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using Ft.Consultorio.ServiceDefaults.Application.Interfaces;
using Ft.Consultorio.ServiceDefaults.Infrastructure.Settings;

namespace Ft.Consultorio.ServiceDefaults.Infrastructure.Storage
{
    public sealed class S3FileStorageService : IFileStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly StorageSettings _settings;
        private readonly ILogger<S3FileStorageService> _logger;

        public S3FileStorageService(
            IAmazonS3 s3Client,
            StorageSettings settings,
            ILogger<S3FileStorageService> logger)
        {
            _s3Client = s3Client;
            _settings = settings;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ErrorOr<bool>> UploadFileAsync(
            string bucketName,
            string objectName,
            string filePath,
            string contentType)
        {
            if (!File.Exists(filePath))
                return Error.NotFound("UploadFileAsync.FileNotFound", "Source file not found.");

            var bucketReady = await EnsureBucketAsync(bucketName);
            if (bucketReady.IsError)
                return bucketReady.Errors.First();

            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = objectName,
                FilePath = filePath,
                ContentType = contentType,
                UseChunkEncoding = false,
                DisablePayloadSigning = true
            };

            await _s3Client.PutObjectAsync(request);

            return true;
        }

        public async Task<ErrorOr<bool>> UploadFileAsync(
            string bucketName,
            string objectName,
            Stream fileStream,
            string contentType)
        {
            if (fileStream == null || !fileStream.CanRead)
                return Error.Validation("UploadFileAsync.InvalidStream", "The provided stream is null or unreadable.");

            if (fileStream.CanSeek)
                fileStream.Seek(0, SeekOrigin.Begin);

            var bucketReady = await EnsureBucketAsync(bucketName);
            if (bucketReady.IsError)
                return bucketReady.Errors.First();

            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = objectName,
                InputStream = fileStream,
                ContentType = contentType,
                UseChunkEncoding = false,
                DisablePayloadSigning = true
            };

            await _s3Client.PutObjectAsync(request);

            return true;
        }

        public async Task<ErrorOr<string>> GetFileUrlAsync(
            string bucketName,
            string objectName,
            int? expirySeconds = null)
        {
            var bucket = _settings.Buckets.FirstOrDefault(b =>
                string.Equals(b.Name, bucketName, StringComparison.OrdinalIgnoreCase));

            if (bucket is null)
                return Error.NotFound("Storage.BucketNotConfigured", "Bucket not configured.");

            var bucketReady = await EnsureBucketAsync(bucketName);
            if (bucketReady.IsError)
                return bucketReady.Errors.First();

            var ttl = expirySeconds ?? bucket.DefaultTtlSeconds;

            try
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = bucketName,
                    Key = objectName,
                    Expires = DateTime.UtcNow.AddSeconds(ttl),
                    Verb = HttpVerb.GET
                };

                var url = _s3Client.GetPreSignedURL(request);
                return url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating presigned GET URL for {Bucket}/{Object}", bucketName, objectName);
                return Error.Failure("GetFileUrlAsync.S3Error", "Failed to generate URL.");
            }
        }

        public async Task<ErrorOr<string>> GetUploadUrlAsync(
            string bucketName,
            string objectName,
            int? expirySeconds = null,
            string? contentType = null)
        {
            var bucket = _settings.Buckets.FirstOrDefault(b =>
                string.Equals(b.Name, bucketName, StringComparison.OrdinalIgnoreCase));

            if (bucket is null)
                return Error.NotFound("Storage.BucketNotConfigured", "Bucket not configured.");

            var bucketReady = await EnsureBucketAsync(bucketName);
            if (bucketReady.IsError)
                return bucketReady.Errors.First();

            var ttl = expirySeconds ?? bucket.DefaultTtlSeconds;

            try
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = bucketName,
                    Key = objectName,
                    Expires = DateTime.UtcNow.AddSeconds(ttl),
                    Verb = HttpVerb.PUT
                };

                if (!string.IsNullOrWhiteSpace(contentType))
                {
                    request.ContentType = contentType;
                }

                var url = _s3Client.GetPreSignedURL(request);
                return url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating presigned PUT URL for {Bucket}/{Object}", bucketName, objectName);
                return Error.Failure("GetUploadUrlAsync.S3Error", "Failed to generate URL.");
            }
        }

        public async Task<ErrorOr<bool>> DeleteFileAsync(string bucketName, string objectName)
        {
            try
            {
                var request = new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = objectName
                };

                await _s3Client.DeleteObjectAsync(request);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting {Bucket}/{Object}", bucketName, objectName);
                return Error.Failure("DeleteFileAsync.S3Error", "An error occurred while deleting the file.");
            }
        }

        private async Task<ErrorOr<bool>> EnsureBucketAsync(string bucketName)
        {
            try
            {
                if (await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName))
                    return true;

                if (_settings.Buckets.Length == 0 ||
                    !_settings.Buckets.Any(b => string.Equals(b.Name, bucketName, StringComparison.OrdinalIgnoreCase)))
                    return Error.NotFound("Storage.BucketNotConfigured", "Bucket not configured.");

                await _s3Client.PutBucketAsync(new PutBucketRequest
                {
                    BucketName = bucketName
                });

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring bucket {Bucket}", bucketName);
                return Error.Failure("Storage.BucketError", "Bucket not found and could not be created.");
            }
        }

        public async Task<ErrorOr<string>> GetCdnUrlAsync(string bucketName, string objectName, int? expirySeconds = null)
        {
            var bucket = _settings.Buckets.FirstOrDefault(b =>
                string.Equals(b.Name, bucketName, StringComparison.OrdinalIgnoreCase));

            if (bucket is null)
                return Error.NotFound("Storage.BucketNotConfigured", "Bucket not configured.");

            if (string.IsNullOrWhiteSpace(bucket.CdnBaseUrl))
                return Error.Failure("Storage.MissingCdnBaseUrl", "Bucket CdnBaseUrl not configured.");

            var baseUrl = bucket.CdnBaseUrl.TrimEnd('/');

            // bucketName/objectName (sin slashes dobles)
            var cleanBucket = bucketName.Trim('/');
            var cleanObject = objectName.TrimStart('/');
            var key = $"{cleanBucket}/{cleanObject}";

            var unsignedUrl = $"{baseUrl}/{key}";

            // Public: URL directa
            if (bucket.Visibility == AssetVisibility.Public)
                return unsignedUrl;

            // Private: firmar
            if (string.IsNullOrWhiteSpace(bucket.SigningSecret))
                return Error.Failure("CDN.MissingSigningSecret", "Bucket SigningSecret not configured.");

            var ttl = expirySeconds ?? bucket.DefaultTtlSeconds;

            // pathname exacto que ve el Worker (sin dominio)
            var pathname = new Uri(unsignedUrl).AbsolutePath;

            var signedUrl = CreateSignedCdnUrl(baseUrl, pathname, bucket.SigningSecret!, ttl);
            return signedUrl;
        }

        private static string CreateSignedCdnUrl(string baseUrl, string pathname, string secret, int expirySeconds)
        {
            if (!pathname.StartsWith("/")) pathname = "/" + pathname;

            var exp = DateTimeOffset.UtcNow.AddSeconds(expirySeconds).ToUnixTimeSeconds();
            var dataToSign = $"{exp}\n{pathname}";
            var sig = HmacSha256Base64Url(secret, dataToSign);

            return $"{baseUrl}{pathname}?exp={exp}&sig={sig}";
        }

        private static string HmacSha256Base64Url(string secret, string data)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hash).Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }
    }
}
