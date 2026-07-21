namespace Ft.Consultorio.MsAuth.Application.Interfaces.Auth
{
    public interface IGoogleBirthDateResolver
    {
        Task<DateOnly?> TryResolveAsync(string? accessToken, CancellationToken cancellationToken);
    }
}
