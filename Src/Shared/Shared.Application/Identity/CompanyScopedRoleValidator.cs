
using Microsoft.AspNetCore.Identity;
using UserManagement.Domain.Entities;

namespace Shared.Application.Identity;


public class CompanyScopedRoleValidator : IRoleValidator<Role>
{
    public async Task<IdentityResult> ValidateAsync(RoleManager<Role> manager, Role role)
    {
        if (string.IsNullOrWhiteSpace(role.Name))
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "InvalidRoleName",
                Description = "Role name cannot be empty."
            });
        }

        var normalizedName = manager.NormalizeKey(role.Name);

        var duplicate = await manager.Roles.AnyAsync(r =>
            r.NormalizedName == normalizedName &&
            r.CompanyId == role.CompanyId &&
            r.Id != role.Id);

        if (duplicate)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "DuplicateRoleName",
                Description = role.CompanyId is null
                    ? $"Global role '{role.Name}' already exists."
                    : $"Role '{role.Name}' already exists for this company."
            });
        }

        return IdentityResult.Success;
    }
}
