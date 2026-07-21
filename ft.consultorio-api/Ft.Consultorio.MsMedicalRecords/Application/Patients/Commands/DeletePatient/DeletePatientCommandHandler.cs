using ErrorOr;
using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.ServiceDefaults.Domain.Primitives;
using Ft.Consultorio.MsMedicalRecords.Application.Interfaces.Repositories;

namespace Ft.Consultorio.MsMedicalRecords.Application.Patients.Commands.DeletePatient
{
    internal sealed class DeletePatientCommandHandler : ApiBaseHandler<DeletePatientCommand, bool>
    {
        private readonly IPatientRepository _patients;
        private readonly IUnitOfWork _unitOfWork;

        public DeletePatientCommandHandler(
            IPatientRepository patients,
            IUnitOfWork unitOfWork,
            ILogger<DeletePatientCommandHandler> logger) : base(logger)
        {
            _patients = patients;
            _unitOfWork = unitOfWork;
        }

        protected override async Task<ErrorOr<ApiResponse<bool>>> HandleRequest(
            DeletePatientCommand request, CancellationToken cancellationToken)
        {
            var patient = await _patients.GetByIdAsync(request.Id, cancellationToken);
            if (patient is null)
            {
                return Error.NotFound("Patient.NotFound", "Patient with the provided Id was not found.");
            }

            _patients.Delete(patient);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<bool>(true, true);
        }
    }
}
