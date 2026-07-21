namespace Ft.Consultorio.ServiceDefaults.Infrastructure.Settings
{
    public class SerilogSettings
    {
        public string? MinimumLevel { get; set; }
        public Dictionary<string, string> Override { get; set; } = new();
        public Dictionary<string, object> Enrich { get; set; } = new();
        public List<WriteToSetting> WriteTo { get; set; } = new();

        public class WriteToSetting
        {
            public string Name { get; set; } = string.Empty;
            public Dictionary<string, object> Args { get; set; } = new();
        }
    }
}
