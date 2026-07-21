using Ft.Consultorio.ServiceDefaults.Application.Interfaces.Repositories;
using Ft.Consultorio.MsAuth.Domain;

namespace Ft.Consultorio.MsAuth.Application.Interfaces.Repositories
{
    public interface IRoleRepository : IBaseRepository<Role, Guid>
    {
        Task<IReadOnlyList<Role>> GetByIdsAsync(IReadOnlyCollection<Guid> roleIds, CancellationToken cancellationToken);
    }
}
