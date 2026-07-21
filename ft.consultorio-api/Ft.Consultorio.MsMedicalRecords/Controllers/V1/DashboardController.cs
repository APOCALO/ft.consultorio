using Asp.Versioning;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.ServiceDefaults.Web.Api.Controllers;
using Ft.Consultorio.ServiceDefaults.Web.Api.Extensions;
using Ft.Consultorio.MsMedicalRecords.Application.Dashboard.DTOs;
using Ft.Consultorio.MsMedicalRecords.Application.Dashboard.Queries.GetDashboard;

namespace Ft.Consultorio.MsMedicalRecords.Controllers.V1
{
    /// <summary>Indicadores del consultorio. Solo administradores.</summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/dashboard")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ApiBaseController
    {
        private readonly ISender _mediator;

        public DashboardController(ISender mediator) => _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

        /// <summary>Devuelve conteos e ingresos agregados.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<DashboardResponseDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetDashboardQuery(), ct);
            return result.Match(ok => Ok(ok), Problem);
        }
    }
}
