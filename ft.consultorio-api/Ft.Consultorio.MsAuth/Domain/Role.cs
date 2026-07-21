using Ft.Consultorio.ServiceDefaults.Domain.Primitives;

namespace Ft.Consultorio.MsAuth.Domain
{
    public class Role : AggregateRoot
    {
        public string Name { get; private set; } = default!;
        public string Description { get; private set; } = default!;

        private Role() { }

        private Role(Guid createdById, string name, string description, Guid? id = null) : base(createdById, id)
        {
            Name = name;
            Description = description;
        }

        public static Role Create(Guid createdById, string name, string description, Guid? id = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Role name is required.", nameof(name));

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Role description is required.", nameof(description));

            return new Role(createdById, name.Trim(), description.Trim(), id);
        }

        public void UpdateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Role name is required.", nameof(name));

            Name = name.Trim();
        }

        public void UpdateDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Role description is required.", nameof(description));

            Description = description.Trim();
        }
    }
}
