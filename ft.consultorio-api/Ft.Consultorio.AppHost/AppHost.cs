var builder = DistributedApplication.CreateBuilder(args);

var postgresPassword = builder.AddParameterFromConfiguration(
    name: "postgres-password",
    configurationKey: "Parameters:postgres-password",
    secret: true);

var cache = builder.AddRedis("cache")
    .WithDataVolume("ftconsultorio-cache-data")
    .WithRedisCommander(); // Agrega interfaz web en puerto aleatorio.

var postgres = builder.AddPostgres("postgres", password: postgresPassword, port: 5432)
    .WithDataVolume("ftconsultorio-postgres-data")
    .WithPgAdmin(); // UI de administración en puerto aleatorio.
var dbAuth = postgres.AddDatabase("db-msauth");
var dbMedicalRecords = postgres.AddDatabase("db-msmedicalrecords");

var msAuth = builder.AddProject<Projects.Ft_Consultorio_MsAuth>("ftconsultorio-auth")
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WithReference(dbAuth, "DatabaseConnection")
    .WaitFor(cache)
    .WaitFor(dbAuth);

var msMedicalRecords = builder.AddProject<Projects.Ft_Consultorio_MsMedicalRecords>("ftconsultorio-medicalrecords")
    .WithHttpHealthCheck("/health")
    .WithReference(dbMedicalRecords, "DatabaseConnection")
    .WaitFor(dbMedicalRecords);

// Agregar el API Gateway
builder.AddProject<Projects.Ft_Consultorio_Gateway>("ftconsultorio-gateway")
    .WithHttpHealthCheck("/health")
    .WithHttpsEndpoint(port: 8080, name: "gateway-https")
    .WithHttpEndpoint(port: 8081, name: "gateway-http")
    .WithReference(msAuth)
    .WithReference(msMedicalRecords)
    .WaitFor(msAuth)
    .WaitFor(msMedicalRecords);

builder.Build().Run();
