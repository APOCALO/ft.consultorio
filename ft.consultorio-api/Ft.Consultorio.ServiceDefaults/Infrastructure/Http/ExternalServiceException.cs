using System.Net;

namespace Ft.Consultorio.ServiceDefaults.Infrastructure.Http
{
    public sealed class ExternalServiceException : Exception
    {
        public ExternalServiceException(string serviceName, HttpStatusCode statusCode, string? responseBody)
            : base(BuildMessage(serviceName, statusCode, responseBody))
        {
            ServiceName = serviceName;
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }

        public string ServiceName { get; }
        public HttpStatusCode StatusCode { get; }
        public string? ResponseBody { get; }

        private static string BuildMessage(string serviceName, HttpStatusCode statusCode, string? responseBody)
        {
            return $"Upstream service '{serviceName}' returned {(int)statusCode} ({statusCode}).";
        }
    }
}
