using Ft.Consultorio.ServiceDefaults.Application.Interfaces.Repositories;
using Ft.Consultorio.MsAuth.Domain;

namespace Ft.Consultorio.MsAuth.Application.Interfaces.Repositories
{
    public interface IPasswordResetTokenRepository : IBaseRepository<PasswordResetToken, Guid>
    {
        Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken);
        Task InvalidateActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    }
}
