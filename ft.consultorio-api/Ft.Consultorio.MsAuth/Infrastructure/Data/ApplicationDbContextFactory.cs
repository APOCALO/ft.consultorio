// Infraestructura/Persistencia/Data/ApplicationDbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ft.Consultorio.MsAuth.Infrastructure.Data
{
    public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // 1) Carga config buscando appsettings.* desde el cwd hacia arriba.
            var basePath = Directory.GetCurrentDirectory();
            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables();

            // Si estás ejecutando desde Infraestructura, intenta también Web.Api/
            var webApiPath = Path.Combine(basePath, "..", "Web.Api");
            if (Directory.Exists(webApiPath))
            {
                builder.AddJsonFile(Path.Combine(webApiPath, "appsettings.json"), optional: true);
                builder.AddJsonFile(Path.Combine(webApiPath, "appsettings.Development.json"), optional: true);
            }

            var config = builder.Build();

            // 2) Prioriza env var DB_CONN, ConnectionStrings:DatabaseConnection.
            var conn =
                Environment.GetEnvironmentVariable("DB_CONN")
                ?? config.GetConnectionString("DatabaseConnection");

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(conn, sql =>
                {
                    sql.EnableRetryOnFailure();
                    sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                })
                .Options;

            // 3) Usa el ctor mínimo (no requiere Publisher/Logger).
            return new ApplicationDbContext(options);
        }
    }
}
