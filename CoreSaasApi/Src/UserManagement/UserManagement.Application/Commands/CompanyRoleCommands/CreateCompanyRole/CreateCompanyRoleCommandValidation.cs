namespace UserManagement.Application.Commands.RoleCommands.CreateRole;

public class CreateCompanyRoleCommandValidation : AbstractValidator<CreateCompanyRoleCommand>
{
    public CreateCompanyRoleCommandValidation()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required.")
            .MaximumLength(100).WithMessage("Role name cannot exceed 100 characters.");
        RuleFor(x => x.Desc)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
        RuleFor(x => x.ModulePermissions)
            .NotNull().WithMessage("Module permissions are required.")
            .Must(list => list.Count > 0).WithMessage("At least one module permission must be provided.");
    }
}
