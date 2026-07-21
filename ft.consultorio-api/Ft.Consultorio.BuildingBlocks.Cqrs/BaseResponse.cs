using ErrorOr;
using MediatR;

namespace Ft.Consultorio.ServiceDefaults.Application.Common
{
    public record BaseResponse<T> : IRequest<ErrorOr<ApiResponse<T>>>
    {
    }
}
