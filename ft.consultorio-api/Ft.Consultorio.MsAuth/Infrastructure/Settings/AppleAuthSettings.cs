namespace Ft.Consultorio.MsAuth.Infrastructure.Settings
{
    public sealed class AppleAuthSettings
    {
        /// <summary>
        /// Audiencias válidas del id_token de Apple. En web es el "Services ID"
        /// (p. ej. co.ftconsultorio.web); en apps nativas iOS es el bundle id de la app.
        /// Puede haber varias si hay app nativa + web.
        /// </summary>
        public List<string> ClientIds { get; init; } = new();

        /// <summary>Emisor esperado del id_token. Apple siempre usa este valor.</summary>
        public string Issuer { get; init; } = "https://appleid.apple.com";

        /// <summary>URL del JWKS de Apple (claves públicas para verificar la firma).</summary>
        public string JwksUri { get; init; } = "https://appleid.apple.com/auth/keys";

        /// <summary>Minutos que se cachean las claves públicas de Apple antes de refrescarlas.</summary>
        public int JwksCacheMinutes { get; init; } = 360;
    }
}
