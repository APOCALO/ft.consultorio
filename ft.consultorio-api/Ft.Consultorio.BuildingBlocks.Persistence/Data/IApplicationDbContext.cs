using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Ft.Consultorio.ServiceDefaults.Infrastructure.Data
{
    public interface IApplicationDbContext
    {

        DbSet<TEntity> Set<TEntity>() where TEntity : class;
        EntityEntry Entry(object entity);
        IModel Model { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
