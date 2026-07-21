using Riok.Mapperly.Abstractions;
using Ft.Consultorio.MsMedicalRecords.Application.Payments.DTOs;
using Ft.Consultorio.MsMedicalRecords.Domain.Payments;

namespace Ft.Consultorio.MsMedicalRecords.Application.Mapping
{
    public interface IPaymentsMapper
    {
        PaymentResponseDTO ToResponse(Payment source);
        List<PaymentResponseDTO> ToResponses(IEnumerable<Payment> source);
    }

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    public partial class PaymentsMapper : IPaymentsMapper
    {
        public partial PaymentResponseDTO ToResponse(Payment source);
        public partial List<PaymentResponseDTO> ToResponses(IEnumerable<Payment> source);
    }
}
