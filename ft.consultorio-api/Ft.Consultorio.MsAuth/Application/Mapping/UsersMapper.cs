using Riok.Mapperly.Abstractions;
using Ft.Consultorio.MsAuth.Application.Roles.DTOs;
using Ft.Consultorio.MsAuth.Application.Users.DTOs;
using Ft.Consultorio.MsAuth.Domain;

namespace Ft.Consultorio.MsAuth.Application.Mapping
{
    public interface IUsersMapper
    {
        UserSessionResponseDTO ToUserSessionResponse(User source);
    }

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    public partial class UsersMapper : IUsersMapper
    {
        [MapProperty(nameof(UserRole.RoleId), nameof(RoleResponseDTO.Id))]
        [MapProperty($"{nameof(UserRole.Role)}.{nameof(Role.Name)}", nameof(RoleResponseDTO.Name))]
        [MapProperty($"{nameof(UserRole.Role)}.{nameof(Role.Description)}", nameof(RoleResponseDTO.Description))]
        private partial RoleResponseDTO ToRoleResponse(UserRole source);

        [MapperIgnoreTarget(nameof(UserSessionResponseDTO.IsLocal))]
        [MapperIgnoreTarget(nameof(UserSessionResponseDTO.IsFirstLogin))]
        private partial UserSessionResponseDTO ToUserSessionResponseInternal(User source);

        public UserSessionResponseDTO ToUserSessionResponse(User source)
        {
            var dto = ToUserSessionResponseInternal(source);
            dto.IsLocal = !string.IsNullOrWhiteSpace(source.PasswordHash);
            return dto;
        }
    }
}
