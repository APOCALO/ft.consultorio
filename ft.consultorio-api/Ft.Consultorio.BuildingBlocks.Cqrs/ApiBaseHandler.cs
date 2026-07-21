using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq;

namespace Ft.Consultorio.ServiceDefaults.Application.Common
{
    public abstract class ApiBaseHandler<TRequest, TResponse> : IRequestHandler<TRequest, ErrorOr<ApiResponse<TResponse>>>
        where TRequest : IRequest<ErrorOr<ApiResponse<TResponse>>>
    {
        protected readonly ILogger<ApiBaseHandler<TRequest, TResponse>> _logger;

        protected ApiBaseHandler(ILogger<ApiBaseHandler<TRequest, TResponse>> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ErrorOr<ApiResponse<TResponse>>> Handle(TRequest request, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation("Handling {RequestName}", typeof(TRequest).Name);

            ErrorOr<ApiResponse<TResponse>> response = await HandleRequest(request, cancellationToken);

            stopwatch.Stop();

            if (!response.IsError)
            {
                response.Value.ResponseTime = stopwatch.Elapsed.TotalMilliseconds;
                _logger.LogInformation("{RequestName} processed successfully in {ElapsedMs} ms", typeof(TRequest).Name, stopwatch.Elapsed.TotalMilliseconds);
            }
            else
            {
                _logger.LogError("{RequestName} failed with error codes: {ErrorCodes}", typeof(TRequest).Name, response.Errors.Select(e => e.Code));
            }

            return response;
        }

        protected abstract Task<ErrorOr<ApiResponse<TResponse>>> HandleRequest(TRequest request, CancellationToken cancellationToken);
    }
}
