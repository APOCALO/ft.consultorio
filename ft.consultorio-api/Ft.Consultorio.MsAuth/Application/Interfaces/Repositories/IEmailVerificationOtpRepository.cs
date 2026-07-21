using Ft.Consultorio.ServiceDefaults.Application.Interfaces.Repositories;
using Ft.Consultorio.MsAuth.Domain;

namespace Ft.Consultorio.MsAuth.Application.Interfaces.Repositories
{
    public interface IEmailVerificationOtpRepository : IBaseRepository<EmailVerificationOtp, Guid>
    {
        Task<EmailVerificationOtp?> GetLatestByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<EmailVerificationOtp?> GetLatestByUserIdAsync(Guid userId, bool asNoTracking, CancellationToken cancellationToken);
        Task<EmailVerificationOtp?> GetLatestByUserIdAndPurposeAsync(Guid userId, OtpPurpose purpose, CancellationToken cancellationToken);
        Task<EmailVerificationOtp?> GetLatestByEmailAsync(string email, CancellationToken cancellationToken);
        Task<EmailVerificationOtp?> GetLatestByEmailAsync(string email, bool asNoTracking, CancellationToken cancellationToken);
        Task InvalidateActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        Task InvalidateActiveByUserIdAndPurposeAsync(Guid userId, OtpPurpose purpose, CancellationToken cancellationToken);
    }
}
