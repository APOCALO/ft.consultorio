using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Ft.Consultorio.MsAuth.Application.Interfaces.Clients;
using Ft.Consultorio.MsAuth.Infrastructure.Clients;
using Ft.Consultorio.ServiceDefaults.Application.Interfaces.Repositories;
using Ft.Consultorio.ServiceDefaults.Domain.Primitives;
using Ft.Consultorio.ServiceDefaults.Infrastructure.Data;
using Ft.Consultorio.ServiceDefaults.Infrastructure.Persistence.Repositories;
using Ft.Consultorio.MsAuth.Application.Interfaces.Auth;
using Ft.Consultorio.MsAuth.Application.Interfaces.Repositories;
using Ft.Consultorio.MsAuth.Domain;
using Ft.Consultorio.MsAuth.Infrastructure.Auth;
using Ft.Consultorio.MsAuth.Infrastructure.Data;
using Ft.Consultorio.MsAuth.Infrastructure.Repositories;
using Ft.Consultorio.MsAuth.Infrastructure.Settings;
using Ft.Consultorio.ServiceDefaults.Extensions;
using Ft.Consultorio.MsAuth.Application.Mapping;

namespace Ft.Consultorio.MsAuth
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();
            services.AddMediatRConfig();
            // Pipeline de validación (FluentValidation) del building block CQRS.
            services.AddValidationBehaviorConfig();
            services.AddValidatorsFromAssemblyContaining<ApplicationAssemblyReference>();
            services.AddSingleton<IUsersMapper, UsersMapper>();

            // Agregar base de datos.
            services.AddPersistence(configuration);

            // Auth.
            AddAuth(services, configuration);

            // Notificaciones (OTP, restablecimiento, bienvenida).
            services.AddNotificationsClient(configuration);

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
            var conn = config.GetConnectionString("DatabaseConnection") ?? throw new InvalidOperationException("Missing 'DatabaseConnection'.");

            // Si tu API crea muchos DbContext por request, el pooling reduce GC/allocs.
            services.AddDbContextPool<ApplicationDbContext>((sp, options) =>
            {
                options.UseNpgsql(conn, sql =>
                {
                    // Resiliencia ante errores transitorios (Azure SQL / redes).
                    sql.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);

                    // Command timeout razonable para cargas pesadas.
                    sql.CommandTimeout(60);
                });
            });

            // Exponer la interfaz de contexto y UoW desde el mismo DbContext.
            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

            // Repositorios.
            services.AddScoped(typeof(IBaseRepository<,>), typeof(BaseRepository<,>));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IEmailVerificationOtpRepository, EmailVerificationOtpRepository>();
            services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();

            return services;
        }

        private static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<GoogleAuthSettings>(configuration.GetSection("Auth:Google"));
            services.Configure<AppleAuthSettings>(configuration.GetSection("Auth:Apple"));
            services.Configure<RefreshTokenSettings>(configuration.GetSection("Auth:RefreshTokens"));
            services.Configure<EmailVerificationOtpSettings>(configuration.GetSection("Auth:EmailVerificationOtp"));
            services.Configure<PasswordResetTokenSettings>(configuration.GetSection("Auth:PasswordReset"));
            services.Configure<TurnstileSettings>(configuration.GetSection("Auth:Turnstile"));

            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<IEmailVerificationOtpService, EmailVerificationOtpService>();
            services.AddScoped<IPasswordResetTokenService, PasswordResetTokenService>();
            services.AddScoped<IGoogleIdTokenValidator, GoogleIdTokenValidator>();
            services.AddHttpClient<IGoogleBirthDateResolver, GoogleBirthDateResolver>(client =>
            {
                client.BaseAddress = new Uri("https://people.googleapis.com/v1/");
            });
            services.AddHttpClient<IAppleIdTokenValidator, AppleIdTokenValidator>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
            });
            services.AddHttpClient<ITurnstileVerifier, TurnstileVerifier>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
            });

            services.Configure<PasswordHasherOptions>(options =>
            {
                options.IterationCount = 100_000;
            });
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

            return services;
        }

        private static IServiceCollection AddNotificationsClient(
            this IServiceCollection services, IConfiguration configuration)
        {
            // Por ahora esta solución solo contiene el microservicio de auth. Registramos un
            // adaptador que deja en el log el contenido de las notificaciones (OTP, enlaces de
            // restablecimiento, bienvenida). Cuando se agregue MsNotifications, reemplazar por
            // un AddHttpClient<INotificationsApiClient, NotificationsApiClient>(...) real.
            services.AddSingleton<INotificationsApiClient, LoggingNotificationsApiClient>();

            return services;
        }
    }
}

