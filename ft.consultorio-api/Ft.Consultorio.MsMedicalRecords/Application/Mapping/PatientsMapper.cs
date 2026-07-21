using Riok.Mapperly.Abstractions;
using Ft.Consultorio.MsMedicalRecords.Application.Patients.DTOs;
using Ft.Consultorio.MsMedicalRecords.Domain.Patients;

namespace Ft.Consultorio.MsMedicalRecords.Application.Mapping
{
    public interface IPatientsMapper
    {
        PatientResponseDTO ToResponse(Patient source);
        List<PatientResponseDTO> ToResponses(IEnumerable<Patient> source);
    }

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    public partial class PatientsMapper : IPatientsMapper
    {
        public partial PatientResponseDTO ToResponse(Patient source);
        public partial List<PatientResponseDTO> ToResponses(IEnumerable<Patient> source);
    }
}
