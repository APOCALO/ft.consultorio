using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Ft.Consultorio.ServiceDefaults.Domain.Primitives
{
    public abstract class AggregateRoot : BaseEntity
    {
        private readonly List<DomainEvent> _domainEvents = new();

        // Exponer solo lectura hacia afuera.
        // No mapear ni serializar.
        [NotMapped]
        [JsonIgnore]
        public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected AggregateRoot() : base() { }

        protected AggregateRoot(Guid createdById, Guid? id = null, DateTime? createdAtUtc = null)
            : base(createdById, id, createdAtUtc)
        {
        }

        // Método protegido para levantar eventos desde el agregado.
        protected void Raise(DomainEvent domainEvent)
        {
            // Podrías validar null aquí si quieres ser más estricto.
            _domainEvents.Add(domainEvent);
        }

        // Limpiar tras publicar/flush en el Unit of Work.
        public void ClearDomainEvents() => _domainEvents.Clear();
    }
}
