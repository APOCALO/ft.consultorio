namespace Ft.Consultorio.MsAuth.Domain
{
    public class UserSettings
    {
        private const string DefaultTheme = "system";

        public Guid UserId { get; private set; } // Clave primaria y foránea hacia User.
        public string Theme { get; private set; } = default!;
        public string Language { get; private set; } = default!;
        public bool NotificationsEnabled { get; private set; } = true;

        private UserSettings() { }

        private UserSettings(Guid userId, string theme, string language, bool notificationsEnabled)
        {
            UserId = userId;
            Theme = theme;
            Language = language;
            NotificationsEnabled = notificationsEnabled;
        }

        public static UserSettings Create(Guid userId, string? theme, string? language, bool? notificationsEnabled)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId is required.", nameof(userId));

            var resolvedTheme = NormalizeTheme(theme) ?? DefaultTheme;
            var resolvedLanguage = string.IsNullOrWhiteSpace(language) ? "es" : language.Trim();
            var resolvedNotifications = notificationsEnabled ?? true;

            return new UserSettings(userId, resolvedTheme, resolvedLanguage, resolvedNotifications);
        }

        public void UpdateTheme(string? theme)
        {
            var normalized = NormalizeTheme(theme);
            if (normalized is null) return;
            Theme = normalized;
        }

        public void UpdateLanguage(string? language)
        {
            if (string.IsNullOrWhiteSpace(language)) return;
            Language = language.Trim();
        }

        public void SetNotificationsEnabled(bool? enabled)
        {
            if (enabled is null) return;
            NotificationsEnabled = enabled.Value;
        }

        private static string? NormalizeTheme(string? theme)
        {
            if (string.IsNullOrWhiteSpace(theme)) return null;
            var normalized = theme.Trim().ToLowerInvariant();
            return normalized is "light" or "dark" or "system" ? normalized : null;
        }
    }
}
