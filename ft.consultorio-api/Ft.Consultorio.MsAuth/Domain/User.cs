using Ft.Consultorio.MsAuth.Application.Auth.DomainEvents;
using Ft.Consultorio.ServiceDefaults.Domain.Primitives;

namespace Ft.Consultorio.MsAuth.Domain
{
    public class User : AggregateRoot
    {
        public string AuthUserId { get; private set; } = default!;
        public string? PasswordHash { get; private set; }
        public string UserName { get; private set; } = default!;
        public string UserEmail { get; private set; } = default!;
        public string FirstName { get; private set; } = default!;
        public string LastName { get; private set; } = default!;
        public DateOnly? BirthDate { get; private set; }
        public bool IsAdult => BirthDate is DateOnly bd
            && GetAge(bd, DateOnly.FromDateTime(DateTime.UtcNow)) >= 18;
        public string? AvatarUrl { get; private set; }
        public string? CountryCode { get; private set; }
        public string? PhoneNumber { get; private set; }
        public GenderEnum Gender { get; private set; } = GenderEnum.Unspecified;
        public string FullName => $"{FirstName} {LastName}";
        public bool IsActive { get; private set; } = true;
        public bool IsEmailVerified { get; private set; } = true;
        public ICollection<UserRole> Roles { get; private set; } = new List<UserRole>();
        public UserSettings? Settings { get; private set; }
        public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();
        public ICollection<EmailVerificationOtp> EmailVerificationOtps { get; private set; } = new List<EmailVerificationOtp>();
        public ICollection<PasswordResetToken> PasswordResetTokens { get; private set; } = new List<PasswordResetToken>();
        public DateTime? LastLoginAt { get; private set; }

        private User() { }

        /// <summary>
        /// Registra un inicio de sesión. Devuelve verdadero si se trata del primer inicio de sesión del usuario.
        /// </summary>
        public bool RecordLogin()
        {
            var isFirst = LastLoginAt is null;
            LastLoginAt = DateTime.UtcNow;
            return isFirst;
        }

        /// <summary>
        /// Levanta el evento de dominio UserRegisteredDomainEvent para notificar el primer registro del usuario.
        /// </summary>
        public void RaiseUserRegistered()
        {
            Raise(new UserRegisteredDomainEvent(Id, UserEmail, FirstName));
        }

        private User(
            Guid createdById,
            string authUserId,
            string userName,
            string userEmail,
            string firstName,
            string lastName,
            DateOnly? birthDate,
            string? avatarUrl,
            bool isActive,
            bool isEmailVerified,
            UserSettings? settings,
            string? passwordHash = null,
            Guid? id = null) : base(createdById, id)
        {
            AuthUserId = authUserId;
            PasswordHash = string.IsNullOrWhiteSpace(passwordHash) ? null : passwordHash;
            UserName = userName;
            UserEmail = userEmail;
            FirstName = firstName;
            LastName = lastName;
            BirthDate = birthDate;
            AvatarUrl = avatarUrl;
            IsActive = isActive;
            IsEmailVerified = isEmailVerified;
            Settings = settings;
        }

        public static User Create(
            Guid? createdById,
            string authUserId,
            string userName,
            string userEmail,
            string firstName,
            string lastName,
            DateOnly? birthDate = null,
            string? avatarUrl = null,
            bool isActive = true,
            bool isEmailVerified = true,
            IEnumerable<Role>? roles = null,
            UserSettings? settings = null,
            string? passwordHash = null,
            Guid? id = null)
        {
            if (string.IsNullOrWhiteSpace(authUserId))
                throw new ArgumentException("AuthUserId is required.", nameof(authUserId));

            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("UserName is required.", nameof(userName));

            if (string.IsNullOrWhiteSpace(userEmail))
                throw new ArgumentException("UserEmail is required.", nameof(userEmail));

            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("FirstName is required.", nameof(firstName));

            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("LastName is required.", nameof(lastName));

            var user = new User(
                createdById ?? Guid.Empty,
                authUserId.Trim(),
                userName.Trim(),
                userEmail.Trim(),
                firstName.Trim(),
                lastName.Trim(),
                birthDate,
                string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl.Trim(),
                isActive,
                isEmailVerified,
                settings,
                passwordHash,
                id);

            if (roles is not null)
            {
                user.SetRoles(roles);
            }

            return user;
        }

        public void UpdateUserName(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("UserName is required.", nameof(userName));

            UserName = userName.Trim();
        }

        public void UpdateUserEmail(string userEmail)
        {
            if (string.IsNullOrWhiteSpace(userEmail))
                throw new ArgumentException("UserEmail is required.", nameof(userEmail));

            UserEmail = userEmail.Trim();
        }

        public void UpdateFirstName(string firstName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("FirstName is required.", nameof(firstName));

            FirstName = firstName.Trim();
        }

        public void UpdateLastName(string lastName)
        {
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("LastName is required.", nameof(lastName));

            LastName = lastName.Trim();
        }

        public void UpdateAvatarUrl(string? avatarUrl)
        {
            AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl.Trim();
        }

        public void UpdateBirthDate(DateOnly? birthDate)
        {
            BirthDate = birthDate;
        }

        // El código de país y el número van juntos: si falta cualquiera, se limpian ambos.
        public void UpdatePhone(string? countryCode, string? phoneNumber)
        {
            var code = string.IsNullOrWhiteSpace(countryCode) ? null : countryCode.Trim();
            var number = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();

            if (code is null || number is null)
            {
                CountryCode = null;
                PhoneNumber = null;
                return;
            }

            CountryCode = code;
            PhoneNumber = number;
        }

        public void UpdateGender(GenderEnum gender)
        {
            Gender = gender;
        }

        private static int GetAge(DateOnly birthDate, DateOnly today)
        {
            var age = today.Year - birthDate.Year;
            if (today < birthDate.AddYears(age)) age--;
            return age;
        }

        public void SetPasswordHash(string? passwordHash)
        {
            PasswordHash = string.IsNullOrWhiteSpace(passwordHash) ? null : passwordHash;
        }

        public void MarkEmailAsVerified() => IsEmailVerified = true;
        public void MarkEmailAsUnverified() => IsEmailVerified = false;
        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;

        public void SetRoles(IEnumerable<Role> roles)
        {
            if (roles is null) throw new ArgumentNullException(nameof(roles));

            var distinctRoles = roles
                .Where(r => r is not null)
                .GroupBy(r => r.Id)
                .Select(g => g.First())
                .ToList();

            Roles = distinctRoles
                .Select(role => UserRole.Create(this, role))
                .ToList();
        }

        public void AddRole(Role role)
        {
            if (role is null) throw new ArgumentNullException(nameof(role));

            if (Roles.Any(r => r.RoleId == role.Id)) return;
            Roles.Add(UserRole.Create(this, role));
        }

        public void RemoveRole(Role role)
        {
            if (role is null) throw new ArgumentNullException(nameof(role));

            var existing = Roles.FirstOrDefault(r => r.RoleId == role.Id);
            if (existing is not null)
            {
                Roles.Remove(existing);
            }
        }

        public void SetSettings(UserSettings? settings)
        {
            Settings = settings;
        }
    }
}
