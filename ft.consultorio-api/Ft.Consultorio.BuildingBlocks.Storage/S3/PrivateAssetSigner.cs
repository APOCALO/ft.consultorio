using System.Diagnostics.CodeAnalysis;
using ErrorOr;
using Ft.Consultorio.ServiceDefaults.Application.Interfaces;
using Ft.Consultorio.ServiceDefaults.Infrastructure.Settings;

namespace Ft.Consultorio.ServiceDefaults.Infrastructure.Storage
{
    public sealed record SignedAssetUrl(string Url, int ExpiresInSeconds);

    public interface IPrivateAssetSigner
    {
        Task<ErrorOr<SignedAssetUrl>> GetSignedReadUrlAsync(
            string bucketName,
            string objectKey,
            int? ttlSeconds = null);

        Task<ErrorOr<SignedAssetUrl>> GetSignedUploadUrlAsync(
            string bucketName,
            string objectKey,
            int? ttlSeconds = null,
            string? contentType = null);
    }

    public sealed class PrivateAssetSigner : IPrivateAssetSigner
    {
        private readonly IFileStorageService _storage;
        private readonly IReadOnlyDictionary<string, StorageBucketSettings> _buckets;

        public PrivateAssetSigner(IFileStorageService storage, StorageSettings settings)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _buckets = settings.Buckets.ToDictionary(
                b => b.Name,
                b => b,
                StringComparer.OrdinalIgnoreCase);
        }

        public async Task<ErrorOr<SignedAssetUrl>> GetSignedReadUrlAsync(
            string bucketName,
            string objectKey,
            int? ttlSeconds = null)
        {
            if (!TryGetPrivateBucket(bucketName, out var bucket))
            {
                return Error.Forbidden("Assets.BucketNotPrivate", "Bucket is not private.");
            }

            var ttl = ttlSeconds ?? bucket.DefaultTtlSeconds;
            var urlResult = await _storage.GetFileUrlAsync(bucketName, objectKey, ttl);

            return urlResult.IsError
                ? urlResult.Errors
                : new SignedAssetUrl(urlResult.Value, ttl);
        }

        public async Task<ErrorOr<SignedAssetUrl>> GetSignedUploadUrlAsync(
            string bucketName,
            string objectKey,
            int? ttlSeconds = null,
            string? contentType = null)
        {
            if (!TryGetPrivateBucket(bucketName, out var bucket))
            {
                return Error.Forbidden("Assets.BucketNotPrivate", "Bucket is not private.");
            }

            var ttl = ttlSeconds ?? bucket.DefaultTtlSeconds;
            var urlResult = await _storage.GetUploadUrlAsync(bucketName, objectKey, ttl, contentType);

            return urlResult.IsError
                ? urlResult.Errors
                : new SignedAssetUrl(urlResult.Value, ttl);
        }

        private bool TryGetPrivateBucket(string bucketName, [NotNullWhen(true)] out StorageBucketSettings? bucket)
        {
            if (_buckets.TryGetValue(bucketName, out bucket) && bucket.Visibility == AssetVisibility.Private)
            {
                return true;
            }

            bucket = null;
            return false;
        }
    }
}
