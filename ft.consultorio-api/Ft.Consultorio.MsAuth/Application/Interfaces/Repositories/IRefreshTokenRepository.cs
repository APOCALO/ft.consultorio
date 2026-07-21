using Ft.Consultorio.ServiceDefaults.Application.Interfaces.Repositories;
using Ft.Consultorio.MsAuth.Domain;

namespace Ft.Consultorio.MsAuth.Application.Interfaces.Repositories
{
    public interface IRefreshTokenRepository : IBaseRepository<RefreshToken, Guid>
    {
        Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken);
        Task<List<RefreshToken>> GetActiveTokensByUserAsync(Guid userId, CancellationToken cancellationToken);
        Task RevokeAllActiveForUserAsync(Guid userId, string reason, CancellationToken cancellationToken);
        Task RevokeOldestActiveTokensAsync(Guid userId, int maxActiveTokens, CancellationToken cancellationToken);
    }
}
