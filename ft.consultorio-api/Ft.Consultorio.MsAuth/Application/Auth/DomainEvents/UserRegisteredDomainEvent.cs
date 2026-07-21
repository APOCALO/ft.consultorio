using Ft.Consultorio.ServiceDefaults.Domain.Primitives;

namespace Ft.Consultorio.MsAuth.Application.Auth.DomainEvents
{
    public record UserRegisteredDomainEvent(
        Guid UserId,
        string Email,
        string FirstName) : DomainEvent;
}
