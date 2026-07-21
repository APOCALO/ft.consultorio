using Asp.Versioning;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.ServiceDefaults.Web.Api.Controllers;
using Ft.Consultorio.ServiceDefaults.Web.Api.Extensions;
using Ft.Consultorio.MsMedicalRecords.Application.MedicalRecords.Commands.UpdateMedicalRecord;
using Ft.Consultorio.MsMedicalRecords.Application.MedicalRecords.DTOs;
using Ft.Consultorio.MsMedicalRecords.Application.Sessions.Commands.CreateSession;
using Ft.Consultorio.MsMedicalRecords.Application.Sessions.DTOs;
using Ft.Consultorio.MsMedicalRecords.Application.Sessions.Queries.GetSessionsByRecord;

namespace Ft.Consultorio.MsMedicalRecords.Controllers.V1
{
    /// <summary>Historia clínica y sus sesiones. Solo administradores.</summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/medical-records")]
    [Authorize(Roles = "Admin")]
    public class MedicalRecordsController : ApiBaseController
    {
        private readonly ISender _mediator;

        public MedicalRecordsController(ISender mediator) => _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

        /// <summary>Actualiza la historia clínica.</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<MedicalRecordResponseDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMedicalRecordCommand command, CancellationToken ct)
        {
            if (command.Id != id)
                return Problem([Error.Validation("MedicalRecord.UpdateInvalid", "The request Id does not match the url Id.")]);

            var userId = User.GetUserId();
            if (userId is null) return Unauthorized(new { message = "Invalid or missing user identifier in token." });

            command = command with { UpdatedById = userId.Value };
            var result = await _mediator.Send(command, ct);
            return result.Match(ok => Ok(ok), Problem);
        }

        /// <summary>Registra una sesión en la historia clínica.</summary>
        [HttpPost("{recordId:guid}/sessions")]
        [ProducesResponseType(typeof(ApiResponse<SessionResponseDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateSession(Guid recordId, [FromBody] CreateSessionCommand command, CancellationToken ct)
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized(new { message = "Invalid or missing user identifier in token." });

            command = command with { MedicalRecordId = recordId, CreatedById = userId.Value };
            var result = await _mediator.Send(command, ct);
            return result.Match(ok => Ok(ok), Problem);
        }

        /// <summary>Lista las sesiones de la historia clínica.</summary>
        [HttpGet("{recordId:guid}/sessions")]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SessionResponseDTO>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSessions(Guid recordId, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetSessionsByRecordQuery(recordId), ct);
            return result.Match(ok => Ok(ok), Problem);
        }
    }
}
