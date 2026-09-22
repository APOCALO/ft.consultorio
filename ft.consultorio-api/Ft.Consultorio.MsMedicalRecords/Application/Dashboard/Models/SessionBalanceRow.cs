namespace Ft.Consultorio.MsMedicalRecords.Application.Dashboard.Models
{
    /// <summary>Sesión con el paciente, para el desglose de ingresos y saldo.</summary>
    public sealed record SessionBalanceRow(
        Guid SessionId,
        Guid PatientId,
        string PatientName,
        DateTime Date,
        DateTime? PaidAt,
        decimal Price);
}
