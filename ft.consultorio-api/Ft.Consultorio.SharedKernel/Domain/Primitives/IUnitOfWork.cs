namespace Ft.Consultorio.ServiceDefaults.Domain.Primitives
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
