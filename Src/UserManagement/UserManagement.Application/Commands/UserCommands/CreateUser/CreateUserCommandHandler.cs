





//using System.Web;
//using UserManagement.Domain.Events.user;

//namespace UserManagement.Application.Commands.UserCommands.CreateUser;

//public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Response<UserViewModel>>
//{
//    private readonly IUnitOfWork _unitOfWork;
//    private readonly IMapper _mapper;
//    private readonly ICurrentUserService _currentUserService;
//    private readonly UserManager<User> _userManager;
//    private readonly IPublisher _publisher;
//    private readonly MailConfig _mailConfig;

//    public CreateUserCommandHandler(
//        IUnitOfWork unitOfWork,
//        IMapper mapper,
//        ICurrentUserService currentUserService,
//        UserManager<User> userManager,
//        IPublisher publisher,
//        MailConfig mailConfig)
//    {
//        _unitOfWork = unitOfWork;
//        _mapper = mapper;
//        _currentUserService = currentUserService;
//        _userManager = userManager;
//        _publisher = publisher;
//        _mailConfig = mailConfig;
//    }

//    public async Task<Response<UserViewModel>> Handle(
//        CreateUserCommand request, CancellationToken cancellationToken)
//    {
//        try
//        {
//            // Read caller identity
//            var userInfo = _currentUserService.UserInfo
//                ?? throw new UnauthorizedAccessException();

//            var companyId = userInfo.CompanyId;
//            var callerUserId = userInfo.Id;

//            // FIX #5: AgentId now comes from claims
//            var agentId = _currentUserService.AgentId
//                ?? throw new UnauthorizedAccessException();

//            // Validate inputs
//            if (request.Roles == null || request.Roles.Count < 1)
//                return Response<UserViewModel>.FailureResponse(Errors.RoleIsRequired);

//            // Fetch repositories
//            var companyRepo = _unitOfWork.Repository<Company>();
//            var agentRepo = _unitOfWork.Repository<Agent>();
//            var agentRoleRepo = _unitOfWork.Repository<AgentRole>();
//            var modulePermissionRepo = _unitOfWork.Repository<ModulePermission>();

//            // Fetch domain objects — all with explicit companyId scoping
//            var company = await companyRepo.GetSingleOrDefaultAsync(
//                x => x.Id == companyId, cancellationToken: cancellationToken);

//            if (company is null)
//                return Response<UserViewModel>.FailureResponse(Errors.CompanyNotFound);

//            var agent = await agentRepo.GetSingleOrDefaultAsync(
//                x => x.Id == agentId && x.CompanyId == companyId, cancellationToken: cancellationToken);

//            if (agent is null)
//                return Response<UserViewModel>.FailureResponse(Errors.AgentNotFound);

//            // Build domain entity
//            // FIX #2 result: pass callerUserId (Guid) not a User navigation object
//            var user = new User(
//                company,
//                companyId,
//                request.UserName,
//                request.FirstName,
//                request.MiddleName,
//                request.LastName,
//                request.Email,
//                request.Contact,
//                entryByUserId: callerUserId);

//            user.AddToAgent(agent);

//            // Assign roles
//            var agentRoles = agentRoleRepo
//                .GetAll(
//                    x => request.Roles.Contains(x.Id) && x.CompanyId == companyId,
//                    includes: x => x.Role!)
//                .ToList();

//            if (agentRoles.Count == 0)
//                return Response<UserViewModel>.FailureResponse(Errors.RoleIsRequired);

//            agentRoles.ForEach(ar => user.AddRole(ar.Role!));

//            // Assign module permissions
//            var permissions = await modulePermissionRepo.GetListAsync(
//                x => request.ModulePermissions.Contains(x.Id),
//                cancellationToken: cancellationToken);

//            permissions.ForEach(user.AddModulePermission);

//            // Persist via Identity
//            var identityResult = await _userManager.CreateAsync(user);
//            if (!identityResult.Succeeded)
//            {
//                return Response<UserViewModel>.FailureResponse(
//                    identityResult.Errors
//                        .Select(e => new ErrorDTO(e.Code, e.Description))
//                        .ToArray());
//            }

//            // Save child collections (UserRoles, AgentUsers, etc.)
//            await _unitOfWork.SaveChangesAsync(cancellationToken);

//            // FIX #3: Post-commit domain event dispatch
//            // Now the user is safely in the DB, generate the token and send email
//            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
//            var resetUrl = _mailConfig.OfficeURL
//                + string.Format(_mailConfig.OfficeNewUserUrl,
//                    HttpUtility.UrlEncode(token),
//                    HttpUtility.UrlEncode(request.Email));

//            // Dispatch AFTER save — email goes out only for persisted users
//            await _publisher.Publish(
//                new UserCreatedEvent(user.FullName, user.UserName, user.Email, resetUrl),
//                cancellationToken);

//            // Map to ViewModel
//            var viewModel = _mapper.Map<UserViewModel>(user);

//            // Remap role IDs to AgentRole IDs
//            foreach (var role in viewModel.RoleList)
//                role.Id = agentRoles.Single(ar => ar.Role!.Id == role.Id).Id;

//            return Response<UserViewModel>.SuccessResponse(viewModel);
//        }
//        catch (Exception ex)
//        {
//            return Response<UserViewModel>.FailureResponse(Errors.Exception(ex));
//        }
//    }
//}






using System.Web;
using UserManagement.Domain.Events.user;

namespace UserManagement.Application.Commands.UserCommands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Response<UserViewModel>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly UserManager<User> _userManager;
    private readonly IPublisher _publisher;
    private readonly MailConfig _mailConfig;

    public CreateUserCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService,
        UserManager<User> userManager,
        IPublisher publisher,
        MailConfig mailConfig)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _userManager = userManager;
        _publisher = publisher;
        _mailConfig = mailConfig;
    }

    public async Task<Response<UserViewModel>> Handle(
        CreateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Read caller identity
            var userInfo = _currentUserService.UserInfo
                ?? throw new UnauthorizedAccessException();

            var companyId = userInfo.CompanyId;
            var callerUserId = userInfo.Id;

            // AgentId comes from claims
            var agentId = _currentUserService.AgentId
                ?? throw new UnauthorizedAccessException();

            // Validate inputs
            if (request.Roles == null || request.Roles.Count < 1)
                return Response<UserViewModel>.FailureResponse(Errors.RoleIsRequired);

            // Fetch repositories
            var companyRepo = _unitOfWork.Repository<Company>();
            var agentRepo = _unitOfWork.Repository<Agent>();
            var agentRoleRepo = _unitOfWork.Repository<AgentRole>();
            var modulePermissionRepo = _unitOfWork.Repository<ModulePermission>();

            // Fetch domain objects — all with explicit companyId scoping
            var company = await companyRepo.GetSingleOrDefaultAsync(
                x => x.Id == companyId, cancellationToken: cancellationToken);

            if (company is null)
                return Response<UserViewModel>.FailureResponse(Errors.CompanyNotFound);

            var agent = await agentRepo.GetSingleOrDefaultAsync(
                x => x.Id == agentId && x.CompanyId == companyId, cancellationToken: cancellationToken);

            if (agent is null)
                return Response<UserViewModel>.FailureResponse(Errors.AgentNotFound);

            // Build domain entity
            // FIX: User's constructor takes companyId (scalar) only — no
            // Company navigation argument. Passing the Company object here
            // (as this used to) risks EF's change tracker treating an
            // untracked/unfamiliar Company instance as a new entity to
            // INSERT during SaveChanges, colliding with the identity column.
            var user = new User(
                request.UserName,
                request.FirstName,
                request.MiddleName,
                request.LastName,
                request.Email,
                request.Contact,
                callerUserId,
                companyId);

            user.AddToAgent(agent);

            // Assign roles
            var agentRoles = agentRoleRepo
                .GetAll(
                    x => request.Roles.Contains(x.Id) && x.CompanyId == companyId,
                    includes: x => x.Role!)
                .ToList();

            if (agentRoles.Count == 0)
                return Response<UserViewModel>.FailureResponse(Errors.RoleIsRequired);

            agentRoles.ForEach(ar => user.AddRole(ar.Role!));

            // Assign module permissions
            var permissions = await modulePermissionRepo.GetListAsync(
                x => request.ModulePermissions.Contains(x.Id),
                cancellationToken: cancellationToken);

            permissions.ForEach(user.AddModulePermission);

            // Persist via Identity
            var identityResult = await _userManager.CreateAsync(user);
            if (!identityResult.Succeeded)
            {
                return Response<UserViewModel>.FailureResponse(
                    identityResult.Errors
                        .Select(e => new ErrorDTO(e.Code, e.Description))
                        .ToArray());
            }

            // Save child collections (UserRoles, AgentUsers, etc.)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Post-commit domain event dispatch
            // Now the user is safely in the DB, generate the token and send email
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetUrl = _mailConfig.OfficeURL
                + string.Format(_mailConfig.OfficeNewUserUrl,
                    HttpUtility.UrlEncode(token),
                    HttpUtility.UrlEncode(request.Email));

            // Dispatch AFTER save — email goes out only for persisted users
            await _publisher.Publish(
                new UserCreatedEvent(user.FullName, user.UserName, user.Email, resetUrl),
                cancellationToken);

            // Map to ViewModel
            var viewModel = _mapper.Map<UserViewModel>(user);

            // Remap role IDs to AgentRole IDs
            foreach (var role in viewModel.RoleList)
                role.Id = agentRoles.Single(ar => ar.Role!.Id == role.Id).Id;

            return Response<UserViewModel>.SuccessResponse(viewModel);
        }
        catch (Exception ex)
        {
            return Response<UserViewModel>.FailureResponse(Errors.Exception(ex));
        }
    }
}
