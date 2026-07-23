namespace UserManagement.Application.Commands.RoleCommands.CreateRole;

public class CreateCompanyRoleHandler : IRequestHandler<CreateCompanyRoleCommand, Response<RoleViewModel>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public CreateCompanyRoleHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<Response<RoleViewModel>> Handle(CreateCompanyRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            int companyId = _currentUserService.CompanyId ?? throw new UnauthorizedAccessException();
            var owner = await _unitOfWork.Repository<UserRole>().GetSingleOrDefaultAsync(x => x.UserId == _currentUserService.UserId && x.CompanyId == companyId && x.Role!.Name == "Owner", cancellationToken: cancellationToken);

            if (owner is null)
            {
                return Response<RoleViewModel>.FailureResponse("You are not authorized to create a role.");
            }

            var company = _unitOfWork.Repository<Company>().GetAll(x => x.Id == companyId, null, x => x.RolesForAgent).FirstOrDefault() ?? throw new UnauthorizedAccessException(); //load company with roles 
            var duplicateRole = _unitOfWork.Repository<Role>().GetAll(x => x.Name == request.Name && x.CompanyId == companyId && x.ToDate == null).FirstOrDefault();
            if (duplicateRole is null)
            {
                return Response<RoleViewModel>.FailureResponse(Errors.RoleAlreadyExists);
            }

            Role role = new Role(request.Name, request.Desc);
            if (request.ModulePermissions.Count > 0)
            {


                var modulePermissionRepo = _unitOfWork.Repository<ModulePermission>();
                IEnumerable<ModulePermission> modulePermissions = modulePermissionRepo
                    .GetAll(e => request.ModulePermissions.Contains(e.Id));
                foreach (var permission in modulePermissions)
                {
                    role.AddRoleModulePermission(permission);
                }
            }
            CompanyRole companyRole = new(role);
            company.AddCompanyRole(companyRole);
            _ = await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Response<RoleViewModel>.SuccessResponse(_mapper.Map<RoleViewModel>(companyRole), Messages.SavedSuccessfully);

        }
        catch (Exception ex)
        {
            return Response<RoleViewModel>.FailureResponse(Errors.Exception(ex));
        }
    }
}
