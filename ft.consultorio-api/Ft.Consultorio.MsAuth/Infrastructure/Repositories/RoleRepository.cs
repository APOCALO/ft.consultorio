using Microsoft.EntityFrameworkCore;
using Ft.Consultorio.ServiceDefaults.Infrastructure.Persistence.Repositories;
using Ft.Consultorio.MsAuth.Application.Interfaces.Repositories;
using Ft.Consultorio.MsAuth.Domain;
using Ft.Consultorio.MsAuth.Infrastructure.Data;

namespace Ft.Consultorio.MsAuth.Infrastructure.Repositories
{
    public class RoleRepository : BaseRepository<Role, Guid>, IRoleRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public RoleRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<Role>> GetByIdsAsync(IReadOnlyCollection<Guid> roleIds, CancellationToken cancellationToken)
        {
            return await _dbContext.Roles
                .Where(r => roleIds.Contains(r.Id))
                .ToListAsync(cancellationToken);
        }
    }
}
