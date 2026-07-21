using Ft.Consultorio.ServiceDefaults.Infrastructure.Storage;

namespace Ft.Consultorio.ServiceDefaults.Infrastructure.Settings
{
    public sealed class StorageSettings
    {
        public string ServiceUrl { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string Region { get; set; } = "auto";
        public bool ForcePathStyle { get; set; } = true;
        public StorageBucketSettings[] Buckets { get; set; } = Array.Empty<StorageBucketSettings>();
    }

    public sealed class StorageBucketSettings
    {
        public string Name { get; set; } = string.Empty;
        public AssetVisibility Visibility { get; set; } = AssetVisibility.Private;
        public string? CdnBaseUrl { get; set; }
        public string? SigningSecret { get; set; }
        public int DefaultTtlSeconds { get; set; } = 3600;
    }
}
