namespace Ft.Consultorio.MsAuth.Domain
{
    /// <summary>
    /// Propósito de un OTP de verificación por correo. Permite reutilizar la misma
    /// tabla para distintos flujos sin que se pisen entre sí.
    /// </summary>
    public enum OtpPurpose
    {
        /// <summary>Verificación del correo durante el registro.</summary>
        Registration = 0,

        /// <summary>Confirmación de propiedad del correo nuevo al cambiarlo.</summary>
        EmailChange = 1
    }
}
