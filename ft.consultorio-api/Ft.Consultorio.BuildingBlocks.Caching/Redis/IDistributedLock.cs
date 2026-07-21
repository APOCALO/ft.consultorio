namespace Ft.Consultorio.ServiceDefaults.Application.Interfaces
{
    public interface IDistributedLock
    {
        /// <summary>
        /// Intenta adquirir un lock. Devuelve un handle desechable si lo obtiene; de lo contrario, null.
        /// </summary>
        Task<IAsyncDisposable?> AcquireAsync(string key, TimeSpan ttl, CancellationToken ct);
    }
}
