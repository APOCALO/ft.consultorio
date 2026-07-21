using Riok.Mapperly.Abstractions;
using Ft.Consultorio.MsMedicalRecords.Application.MedicalRecords.DTOs;
using Ft.Consultorio.MsMedicalRecords.Domain.MedicalRecords;

namespace Ft.Consultorio.MsMedicalRecords.Application.Mapping
{
    public interface IMedicalRecordsMapper
    {
        MedicalRecordResponseDTO ToResponse(MedicalRecord source);
    }

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    public partial class MedicalRecordsMapper : IMedicalRecordsMapper
    {
        [MapProperty(nameof(MedicalRecord.History), nameof(MedicalRecordResponseDTO.History))]
        public partial MedicalRecordResponseDTO ToResponse(MedicalRecord source);

        private partial MedicalHistoryDTO ToHistoryDto(MedicalHistory source);
    }
}
