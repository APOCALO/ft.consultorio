using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ft.Consultorio.ServiceDefaults.Infrastructure.Data;

namespace Ft.Consultorio.ServiceDefaults.Infrastructure.Data
{
    public static class MigrationExtensions
    {
        /// <summary>
        /// Aplica las migraciones pendientes de EF Core.
        /// Debe llamarse solo en ambiente de desarrollo.
        /// </summary>
        public static IApplicationBuilder ApplyMigrations(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
                .CreateLogger("MigrationExtensions");

            var context = scope.ServiceProvider.GetService<IApplicationDbContext>();

            if (context is not DbContext dbContext)
            {
                logger.LogWarning("No IApplicationDbContext registered. Skipping migrations.");
                return app;
            }

            try
            {
                var pending = dbContext.Database.GetPendingMigrations().ToList();

                if (pending.Count > 0)
                {
                    logger.LogInformation("Applying {Count} pending migration(s): {Migrations}",
                        pending.Count, string.Join(", ", pending));
                    dbContext.Database.Migrate();
                    logger.LogInformation("Migrations applied successfully.");
                }
                else
                {
                    logger.LogInformation("No pending migrations.");
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Could not apply migrations. The database may not be available.");
            }

            return app;
        }
    }
}
