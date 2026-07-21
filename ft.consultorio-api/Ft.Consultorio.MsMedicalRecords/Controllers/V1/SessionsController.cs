using Asp.Versioning;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.ServiceDefaults.Web.Api.Controllers;
using Ft.Consultorio.ServiceDefaults.Web.Api.Extensions;
using Ft.Consultorio.MsMedicalRecords.Application.Sessions.Commands.DeleteSession;
using Ft.Consultorio.MsMedicalRecords.Application.Sessions.Commands.UpdateSession;
using Ft.Consultorio.MsMedicalRecords.Application.Sessions.DTOs;

namespace Ft.Consultorio.MsMedicalRecords.Controllers.V1
{
    /// <summary>Operaciones directas sobre sesiones. Solo administradores.</summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/sessions")]
    [Authorize(Roles = "Admin")]
    public class SessionsController : ApiBaseController
    {
        private readonly ISender _mediator;

        public SessionsController(ISender mediator) => _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

        /// <summary>Actualiza una sesión.</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<SessionResponseDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSessionCommand command, CancellationToken ct)
        {
            if (command.Id != id)
                return Problem([Error.Validation("Session.UpdateInvalid", "The request Id does not match the url Id.")]);

            var userId = User.GetUserId();
            if (userId is null) return Unauthorized(new { message = "Invalid or missing user identifier in token." });

            command = command with { UpdatedById = userId.Value };
            var result = await _mediator.Send(command, ct);
            return result.Match(ok => Ok(ok), Problem);
        }

        /// <summary>Elimina una sesión.</summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new DeleteSessionCommand(id), ct);
            return result.Match(_ => NoContent(), Problem);
        }
    }
}
