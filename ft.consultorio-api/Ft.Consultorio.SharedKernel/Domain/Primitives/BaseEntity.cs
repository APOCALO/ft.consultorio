namespace Ft.Consultorio.ServiceDefaults.Domain.Primitives
{
    public abstract class BaseEntity
    {
        // Campo Id base para todas las entidades.
        public Guid Id { get; protected set; }

        // Auditoría.
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public Guid CreatedById { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public Guid? UpdatedById { get; private set; }

        // Ctor protegido sin parámetros para EF.
        protected BaseEntity() { }

        // Ctor conveniente para asegurar auditoría e Id.
        protected BaseEntity(Guid createdById, Guid? id = null, DateTime? createdAtUtc = null)
        {
            Id = id is { } value && value != Guid.Empty ? value : Guid.NewGuid();
            CreatedById = createdById;
            CreatedAt = createdAtUtc ?? DateTime.UtcNow;
        }

        // Set de update (permite inyectar tiempo en pruebas).
        public void SetAuditUpdate(Guid updatedById, DateTime? updatedAtUtc = null)
        {
            UpdatedById = updatedById;
            UpdatedAt = updatedAtUtc ?? DateTime.UtcNow;
        }

        #region Igualdad por Id
        public override bool Equals(object? obj)
        {
            if (obj is not BaseEntity other) return false;
            if (ReferenceEquals(this, other)) return true;
            if (GetType() != other.GetType()) return false;

            // Si alguno no tiene Id válido aún, no considerar iguales.
            if (Id == Guid.Empty || other.Id == Guid.Empty) return false;

            return Id == other.Id;
        }

        public static bool operator ==(BaseEntity? a, BaseEntity? b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }

        public static bool operator !=(BaseEntity? a, BaseEntity? b) => !(a == b);

        public override int GetHashCode()
        {
            // Combinar tipo + Id para evitar colisiones entre jerarquías.
            return HashCode.Combine(GetType(), Id);
        }
        #endregion
    }
}
