using System.Text.Json;
using Ft.Consultorio.ServiceDefaults.Domain.Primitives;

namespace Ft.Consultorio.BuildingBlocks.Persistence.Outbox
{
    /// <summary>
    /// Mensaje del Transactional Outbox: un DomainEvent serializado que se persiste
    /// en la misma transacción que los cambios de negocio, garantizando que ningún
    /// evento se pierda si el proceso muere entre el commit y la publicación.
    /// </summary>
    public sealed class OutboxMessage
    {
        public const int MaxAttempts = 5;

        public Guid Id { get; private set; }
        public string Type { get; private set; } = string.Empty;
        public string Content { get; private set; } = string.Empty;
        public DateTime OccurredOnUtc { get; private set; }
        public DateTime? ProcessedOnUtc { get; private set; }
        public int AttemptCount { get; private set; }
        public string? Error { get; private set; }

        private OutboxMessage() { }

        public static OutboxMessage From(DomainEvent domainEvent)
        {
            var type = domainEvent.GetType();
            return new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = type.AssemblyQualifiedName ?? type.FullName ?? type.Name,
                Content = JsonSerializer.Serialize(domainEvent, type),
                OccurredOnUtc = domainEvent.OccurredOnUtc,
                AttemptCount = 0
            };
        }

        public DomainEvent? Deserialize()
        {
            var type = System.Type.GetType(Type);
            if (type is null)
            {
                return null;
            }

            return JsonSerializer.Deserialize(Content, type) as DomainEvent;
        }

        public void MarkProcessed() => ProcessedOnUtc = DateTime.UtcNow;

        public void MarkFailed(Exception exception)
        {
            AttemptCount++;
            Error = $"{exception.GetType().Name}: {exception.Message}";
        }
    }
}
