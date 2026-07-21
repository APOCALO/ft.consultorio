using Ft.Consultorio.ServiceDefaults.Application.Interfaces.Repositories;
using Ft.Consultorio.MsAuth.Domain;

namespace Ft.Consultorio.MsAuth.Application.Interfaces.Repositories
{
    public interface IUserRepository : IBaseRepository<User, Guid>
    {
        Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken);
        Task<User?> GetByIdWithRolesAsync(Guid id, bool asNoTracking, CancellationToken cancellationToken);
        Task<User?> GetByAuthUserIdAsync(string authUserId, CancellationToken cancellationToken);
        Task<User?> GetByAuthUserIdAsync(string authUserId, bool asNoTracking, CancellationToken cancellationToken);
        Task<User?> GetByEmailAsync(string userEmail, CancellationToken cancellationToken);
        Task<User?> GetByEmailAsync(string userEmail, bool asNoTracking, CancellationToken cancellationToken);
    }
}
