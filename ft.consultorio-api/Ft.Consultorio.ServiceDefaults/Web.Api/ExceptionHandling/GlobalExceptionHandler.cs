using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Ft.Consultorio.ServiceDefaults.Infrastructure.Http;
using MvcProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;

namespace Ft.Consultorio.ServiceDefaults.Web.Api.ExceptionHandling
{
    /// <summary>
    /// Manejo global de excepciones con el patrón nativo de ASP.NET Core (IExceptionHandler
    /// + AddProblemDetails), en reemplazo del middleware manual con serialización propia.
    /// </summary>
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly IProblemDetailsService _problemDetailsService;
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionHandler(
            IProblemDetailsService problemDetailsService,
            ILogger<GlobalExceptionHandler> logger,
            IHostEnvironment env)
        {
            _problemDetailsService = problemDetailsService;
            _logger = logger;
            _env = env;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var (status, title, logLevel) = Map(httpContext, exception);

            _logger.Log(logLevel, exception, "{Title} ({StatusCode}) handling {Method} {Path}.",
                title, status, httpContext.Request.Method, httpContext.Request.Path);

            if (httpContext.Response.HasStarted)
            {
                return true;
            }

            httpContext.Response.StatusCode = status;

            var problem = new MvcProblemDetails
            {
                Status = status,
                Title = title,
                Detail = _env.IsDevelopment() ? exception.Message : null,
                Instance = httpContext.Request.Path
            };

            problem.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;

            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = problem
            });
        }

        private static (int Status, string Title, LogLevel Level) Map(HttpContext context, Exception exception) => exception switch
        {
            OperationCanceledException when context.RequestAborted.IsCancellationRequested
                => (StatusCodes.Status408RequestTimeout, "Client Closed Request", LogLevel.Warning),
            TaskCanceledException
                => (StatusCodes.Status504GatewayTimeout, "Gateway Timeout", LogLevel.Warning),
            ExternalServiceException
                => (StatusCodes.Status502BadGateway, "Upstream service error", LogLevel.Warning),
            HttpRequestException
                => (StatusCodes.Status502BadGateway, "Upstream service error", LogLevel.Warning),
            UnauthorizedAccessException
                => (StatusCodes.Status401Unauthorized, "Unauthorized", LogLevel.Warning),
            KeyNotFoundException
                => (StatusCodes.Status404NotFound, "Not Found", LogLevel.Information),
            _
                => (StatusCodes.Status500InternalServerError, "Server Error", LogLevel.Error)
        };
    }
}
