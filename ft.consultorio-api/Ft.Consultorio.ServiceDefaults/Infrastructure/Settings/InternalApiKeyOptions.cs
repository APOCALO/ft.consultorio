namespace Ft.Consultorio.ServiceDefaults.Infrastructure.Settings
{
    public sealed class InternalApiKeyOptions
    {
        public const string SectionName = "ServiceDefaults:InternalApiKey";
        public string Key { get; init; } = string.Empty;
    }
}
