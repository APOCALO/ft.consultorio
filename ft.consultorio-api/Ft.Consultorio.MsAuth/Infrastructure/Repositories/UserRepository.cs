using Microsoft.EntityFrameworkCore;
using Ft.Consultorio.ServiceDefaults.Infrastructure.Persistence.Repositories;
using Ft.Consultorio.MsAuth.Application.Interfaces.Repositories;
using Ft.Consultorio.MsAuth.Domain;
using Ft.Consultorio.MsAuth.Infrastructure.Data;

namespace Ft.Consultorio.MsAuth.Infrastructure.Repositories
{
    public class UserRepository : BaseRepository<User, Guid>, IUserRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public UserRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken)
        {
            return await GetByIdWithRolesAsync(id, asNoTracking: false, cancellationToken);
        }

        public async Task<User?> GetByIdWithRolesAsync(
            Guid id,
            bool asNoTracking,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.Users
                .Include(u => u.Roles)
                .ThenInclude(ur => ur.Role)
                .Include(u => u.Settings)
                .AsQueryable();

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public async Task<User?> GetByAuthUserIdAsync(string authUserId, CancellationToken cancellationToken)
        {
            return await GetByAuthUserIdAsync(authUserId, asNoTracking: false, cancellationToken);
        }

        public async Task<User?> GetByAuthUserIdAsync(
            string authUserId,
            bool asNoTracking,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.Users
                .Include(u => u.Roles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.Settings)
                .AsQueryable();

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(u => u.AuthUserId == authUserId, cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(string userEmail, CancellationToken cancellationToken)
        {
            return await GetByEmailAsync(userEmail, asNoTracking: false, cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(
            string userEmail,
            bool asNoTracking,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.Users
                .Include(u => u.Roles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.Settings)
                .AsQueryable();

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(u => u.UserEmail == userEmail, cancellationToken);
        }
    }
}
