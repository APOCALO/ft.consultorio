using Asp.Versioning;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.ServiceDefaults.Web.Api.Controllers;
using Ft.Consultorio.ServiceDefaults.Web.Api.Extensions;
using Ft.Consultorio.MsMedicalRecords.Application.Patients.Commands.CreatePatient;
using Ft.Consultorio.MsMedicalRecords.Application.Patients.Commands.DeletePatient;
using Ft.Consultorio.MsMedicalRecords.Application.Patients.Commands.DischargePatient;
using Ft.Consultorio.MsMedicalRecords.Application.Patients.Commands.UpdatePatient;
using Ft.Consultorio.MsMedicalRecords.Application.Patients.DTOs;
using Ft.Consultorio.MsMedicalRecords.Application.Patients.Queries.GetAllPatientsPaged;
using Ft.Consultorio.MsMedicalRecords.Application.Patients.Queries.GetPatientById;
using Ft.Consultorio.MsMedicalRecords.Application.MedicalRecords.Commands.CreateMedicalRecord;
using Ft.Consultorio.MsMedicalRecords.Application.MedicalRecords.DTOs;
using Ft.Consultorio.MsMedicalRecords.Application.MedicalRecords.Queries.GetMedicalRecordByPatient;
using Ft.Consultorio.MsMedicalRecords.Application.Payments.Commands.RegisterPayment;
using Ft.Consultorio.MsMedicalRecords.Application.Payments.DTOs;
using Ft.Consultorio.MsMedicalRecords.Application.Payments.Queries.GetPaymentsByPatient;

namespace Ft.Consultorio.MsMedicalRecords.Controllers.V1
{
    /// <summary>Gestión de pacientes e información clínica asociada. Solo administradores.</summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/patients")]
    [Authorize(Roles = "Admin")]
    public class PatientsController : ApiBaseController
    {
        private readonly ISender _mediator;

        public PatientsController(ISender mediator) => _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

        /// <summary>Lista pacientes paginados (con búsqueda opcional por nombre o documento).</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PatientResponseDTO>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParameters pagination, [FromQuery] string? search, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetAllPatientsPagedQuery(pagination, search), ct);
            return result.Match(ok => Ok(ok), Problem);
        }

        /// <summary>Obtiene un paciente por su identificador.</summary>
        [HttpGet("{id:guid}", Name = nameof(GetPatientById))]
        [ProducesResponseType(typeof(ApiResponse<PatientResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPatientById(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetPatientByIdQuery(id), ct);
            return result.Match(ok => Ok(ok), Problem);
        }

        /// <summary>Crea un nuevo paciente.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PatientResponseDTO>), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreatePatientCommand command, CancellationToken ct)
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized(new { message = "Invalid or missing user identifier in token." });

            command = command with { CreatedById = userId.Value };
            var result = await _mediator.Send(command, ct);
            return result.Match(
                ok => CreatedAtRoute(nameof(GetPatientById), new { id = ok.Data.Id }, ok),
                Problem);
        }

        /// <summary>Actualiza un paciente existente.</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientResponseDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePatientCommand command, CancellationToken ct)
        {
            if (command.Id != id)
                return Problem([Error.Validation("Patient.UpdateInvalid", "The request Id does not match the url Id.")]);

            var userId = User.GetUserId();
            if (userId is null) return Unauthorized(new { message = "Invalid or missing user identifier in token." });

            command = command with { UpdatedById = userId.Value };
            var result = await _mediator.Send(command, ct);
            return result.Match(ok => Ok(ok), Problem);
        }

        /// <summary>Elimina un paciente.</summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new DeletePatientCommand(id), ct);
            return result.Match(_ => NoContent(), Problem);
        }

        /// <summary>Da de alta (discharge) a un paciente.</summary>
        [HttpPost("{id:guid}/discharge")]
        [ProducesResponseType(typeof(ApiResponse<PatientResponseDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Discharge(Guid id, CancellationToken ct)
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized(new { message = "Invalid or missing user identifier in token." });

            var command = new DischargePatientCommand { Id = id, UpdatedById = userId.Value };
            var result = await _mediator.Send(command, ct);
            return result.Match(ok => Ok(ok), Problem);
        }

        /// <summary>Crea la historia clínica del paciente (una por paciente).</summary>
        [HttpPost("{patientId:guid}/medical-record")]
        [ProducesResponseType(typeof(ApiResponse<MedicalRecordResponseDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateMedicalRecord(Guid patientId, [FromBody] CreateMedicalRecordCommand command, CancellationToken ct)
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized(new { message = "Invalid or missing user identifier in token." });

            command = command with { PatientId = patientId, CreatedById = userId.Value };
            var result = await _mediator.Send(command, ct);
            return result.Match(ok => Ok(ok), Problem);
        }

        /// <summary>Obtiene la historia clínica del paciente.</summary>
        [HttpGet("{patientId:guid}/medical-record")]
        [ProducesResponseType(typeof(ApiResponse<MedicalRecordResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMedicalRecord(Guid patientId, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetMedicalRecordByPatientQuery(patientId), ct);
            return result.Match(ok => Ok(ok), Problem);
        }

        /// <summary>Registra un pago del paciente.</summary>
        [HttpPost("{patientId:guid}/payments")]
        [ProducesResponseType(typeof(ApiResponse<PaymentResponseDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RegisterPayment(Guid patientId, [FromBody] RegisterPaymentCommand command, CancellationToken ct)
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized(new { message = "Invalid or missing user identifier in token." });

            command = command with { PatientId = patientId, CreatedById = userId.Value };
            var result = await _mediator.Send(command, ct);
            return result.Match(ok => Ok(ok), Problem);
        }

        /// <summary>Lista los pagos del paciente.</summary>
        [HttpGet("{patientId:guid}/payments")]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PaymentResponseDTO>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPayments(Guid patientId, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetPaymentsByPatientQuery(patientId), ct);
            return result.Match(ok => Ok(ok), Problem);
        }
    }
}
