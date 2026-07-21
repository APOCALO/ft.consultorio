namespace Ft.Consultorio.MsAuth.Domain
{
    public class UserRole
    {
        public Guid UserId { get; private set; }
        public User User { get; private set; } = default!;

        public Guid RoleId { get; private set; }
        public Role Role { get; private set; } = default!;

        private UserRole() { }

        private UserRole(User user, Role role)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
            Role = role ?? throw new ArgumentNullException(nameof(role));
            UserId = user.Id;
            RoleId = role.Id;
        }

        public static UserRole Create(User user, Role role) => new UserRole(user, role);
    }
}
