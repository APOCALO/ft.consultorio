using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ft.Consultorio.MsMedicalRecords.Infrastructure.Data
{
    public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();
            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

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

            return new ApplicationDbContext(options);
        }
    }
}
