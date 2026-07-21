using Ft.Consultorio.ServiceDefaults.Infrastructure.Settings;

namespace Ft.Consultorio.ServiceDefaults.Infrastructure.Storage
{
    public interface IPublicAssetUrlResolver
    {
        string? ResolvePublicUrl(string bucketName, string objectKey);
    }

    public sealed class PublicAssetUrlResolver : IPublicAssetUrlResolver
    {
        private readonly IReadOnlyDictionary<string, StorageBucketSettings> _buckets;

        public PublicAssetUrlResolver(StorageSettings settings)
        {
            _buckets = settings.Buckets.ToDictionary(
                b => b.Name,
                b => b,
                StringComparer.OrdinalIgnoreCase);
        }

        public string? ResolvePublicUrl(string bucketName, string objectKey)
        {
            if (!_buckets.TryGetValue(bucketName, out var bucket)) return null;
            if (bucket.Visibility != AssetVisibility.Public) return null;
            if (string.IsNullOrWhiteSpace(bucket.CdnBaseUrl)) return null;
            if (string.IsNullOrWhiteSpace(objectKey)) return null;

            var baseUrl = bucket.CdnBaseUrl.TrimEnd('/');
            var key = objectKey.TrimStart('/');
            return $"{baseUrl}/{key}";
        }
    }
}
