using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Ft.Consultorio.MsMedicalRecords.Application.Interfaces.Repositories;
using Ft.Consultorio.MsMedicalRecords.Application.Mapping;
using Ft.Consultorio.MsMedicalRecords.Infrastructure.Data;
using Ft.Consultorio.MsMedicalRecords.Infrastructure.Repositories;
using Ft.Consultorio.ServiceDefaults.Application.Interfaces.Repositories;
using Ft.Consultorio.ServiceDefaults.Domain.Primitives;
using Ft.Consultorio.ServiceDefaults.Extensions;
using Ft.Consultorio.ServiceDefaults.Infrastructure.Data;
using Ft.Consultorio.ServiceDefaults.Infrastructure.Persistence.Repositories;

namespace Ft.Consultorio.MsMedicalRecords
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();
            services.AddMediatRConfig();
            services.AddValidationBehaviorConfig();
            services.AddValidatorsFromAssemblyContaining<ApplicationAssemblyReference>();

            services.AddSingleton<IPatientsMapper, PatientsMapper>();
            services.AddSingleton<IMedicalRecordsMapper, MedicalRecordsMapper>();
            services.AddSingleton<ISessionsMapper, SessionsMapper>();
            services.AddSingleton<IPaymentsMapper, PaymentsMapper>();

            services.AddPersistence(configuration);

            return services;
        }

        private static IServiceCollection AddMediatRConfig(this IServiceCollection services)
        {
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssemblyContaining<ApplicationAssemblyReference>();
            });
            return services;
        }

        private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration config)
        {
            var conn = config.GetConnectionString("DatabaseConnection")
                ?? throw new InvalidOperationException("Missing 'DatabaseConnection'.");

            services.AddDbContextPool<ApplicationDbContext>((sp, options) =>
            {
                options.UseNpgsql(conn, sql =>
                {
                    sql.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                    sql.CommandTimeout(60);
                });
            });

            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

            services.AddScoped(typeof(IBaseRepository<,>), typeof(BaseRepository<,>));
            services.AddScoped<IPatientRepository, PatientRepository>();
            services.AddScoped<IMedicalRecordRepository, MedicalRecordRepository>();
            services.AddScoped<ISessionRepository, SessionRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();

            return services;
        }
    }
}
