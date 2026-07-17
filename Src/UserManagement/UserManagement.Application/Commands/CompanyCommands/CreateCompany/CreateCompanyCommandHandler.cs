//using Microsoft.Extensions.Logging;

//namespace UserManagement.Application.Commands.CompanyCommands.CreateCompany;

//public class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, Response<CompanyCreateViewModel>>
//{
//    private readonly IUnitOfWork _unitOfWork;
//    private readonly IMapper _mapper;
//    private readonly UserManager<User> _userManager;
//    private readonly RoleManager<Role> _roleManager;
//    private readonly IMediator _mediator;
//    private readonly ILogger<CreateCompanyCommandHandler> _logger;
//    private readonly MailConfig _mailConfig;

//    public CreateCompanyCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, UserManager<User> userManager, RoleManager<Role> roleManager, IMediator mediator, ILogger<CreateCompanyCommandHandler> logger, MailConfig mailConfig)
//    {
//        _unitOfWork = unitOfWork;
//        _mapper = mapper;
//        _userManager = userManager;
//        _roleManager = roleManager;
//        _mediator = mediator;
//        _logger = logger;
//        _mailConfig = mailConfig;
//    }

//    public async Task<Response<CompanyCreateViewModel>> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
//    {
//        var companyRepo = _unitOfWork.Repository<Company>();

//        var dupCompany = await companyRepo.GetSingleOrDefaultAsync(
//            predicate: c => c.Name == request.Name || c.Email == request.Email || c.Pan == request.Pan || c.RegNo == request.RegNo,
//            disableTracking: true,
//            cancellationToken: cancellationToken);

//        if (dupCompany != null)
//        {
//            return Response<CompanyCreateViewModel>.FailureResponse(Errors.CompanyAlreadyExists);
//        }

//        // 1. Insert Company and SAVE IMMEDIATELY so company.Id is populated
//        //    with the real identity value before anything tries to FK against it.
//        Company company = new(
//            request.ProductCode,
//            request.Name,
//            request.Email,
//            request.Address,
//            request.PhoneNo,
//            request.Pan,
//            request.RegNo,
//            request.Url
//            );

//        companyRepo.Insert(company);

//        // 2. NOW company.Id is a real, committed value — safe to reference from Agent/User
//        Agent agent = new(
//            name: "Main Agent",
//            address: request.Address,
//            pan: request.Pan,
//            regNo: request.RegNo,
//            isParent: true,
//            referralCode: $"{request.RegNo}--{company.Id}",
//            companyId: company.Id
//            );

//        var agentRepo = _unitOfWork.Repository<Agent>();
//        agentRepo.Insert(agent);
//        company.AddAgent(agent);

//        User user = new(
//            companyId: company.Id,
//            userName: request.MainUsername,
//            firstName: request.MainUserFirstName,
//            middleName: null,
//            lastName: request.MainUserLastName,
//            email: request.MainUserEmail,
//            contact: request.MainUserContactNo,
//            entryByUserId: null
//            );

//        user.AddToAgent(agent);

//        var identityResult = await _userManager.CreateAsync(user);
//        if (!identityResult.Succeeded)
//        {
//            return Response<CompanyCreateViewModel>.FailureResponse(Errors.UserAlreadyExists);
//        }

//        // 3. Create default "Owner" role for the company
//        Role ownerRole = new
//            (
//            company.Id,
//            "Owner",
//            "Default owner role with full permissions"
//            );
//        var roleCreationResult = await _roleManager.CreateAsync(ownerRole);
//        if (!roleCreationResult.Succeeded)
//        {
//            return Response<CompanyCreateViewModel>.FailureResponse(Errors.RoleAlreadyExists);
//        }


//        await _unitOfWork.SaveChangesAsync(cancellationToken);

//        return Response<CompanyCreateViewModel>.SuccessResponse(
//            _mapper.Map<CompanyCreateViewModel>(company),
//            "Company created successfully");
//    }
//}



using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace UserManagement.Application.Commands.CompanyCommands.CreateCompany;

public class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, Response<string>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IMediator _mediator;
    private readonly ILogger<CreateCompanyCommandHandler> _logger;
    private readonly MailConfig _mailConfig;

    public CreateCompanyCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IMediator mediator,
        ILogger<CreateCompanyCommandHandler> logger,
        MailConfig mailConfig)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _userManager = userManager;
        _roleManager = roleManager;
        _mediator = mediator;
        _logger = logger;
        _mailConfig = mailConfig;
    }

    public async Task<Response<string>> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
    {
        var companyRepo = _unitOfWork.Repository<Company>();

        var dupCompany = await companyRepo.GetSingleOrDefaultAsync(
            predicate: c => c.Name == request.Name || c.Email == request.Email || c.Pan == request.Pan || c.RegNo == request.RegNo,
            disableTracking: true,
            cancellationToken: cancellationToken);

        if (dupCompany != null)
        {
            return Response<string>.FailureResponse(Errors.CompanyAlreadyExists);
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // 1. Company — insert and flush so company.Id is real before anything
            //    downstream reads it as a scalar FK.
            Company company = new(
                request.ProductCode,
                request.Name,
                request.Email,
                request.Address,
                request.PhoneNo,
                request.Pan,
                request.RegNo,
                request.Url
                );

            companyRepo.Insert(company);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 2. Default Agent (Head Office)
            Agent agent = new(
                name: request.BranchName,
                address: request.BranchAddress,
                pan: request.Pan,
                regNo: request.RegNo,
                isParent: true,
                referralCode: $"{request.RegNo}--{company.Id}",
                companyId: company.Id
                );

            var agentRepo = _unitOfWork.Repository<Agent>();
            agentRepo.Insert(agent);
            company.AddAgent(agent);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 3. Default roles
            Role ownerRole = new
                (
                name: "Owner",
                desc: "Default owner role with full permissions",
                companyId: company.Id
                );
            var ownerRoleResult = await _roleManager.CreateAsync(ownerRole);
            if (!ownerRoleResult.Succeeded)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                return Response<string>.FailureResponse(Errors.RoleAlreadyExists);
            }
            // 4. Owner user — created without a password (Identity allows this).
            //    A reset-token invite link is generated below instead of setting
            //    one server-side, same approach as the old project.
            User user = new(
                companyId: company.Id,
                userName: request.MainUsername,
                firstName: request.MainUserFirstName,
                middleName: null,
                lastName: request.MainUserLastName,
                email: request.MainUserEmail,
                contact: request.MainUserContactNo,
                entryByUserId: null
                );

            user.AddToAgent(agent);

            var identityResult = await _userManager.CreateAsync(user);
            if (!identityResult.Succeeded)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                return Response<string>.FailureResponse(Errors.UserAlreadyExists);
            }
            // 5. Assign Owner role in userRole table
            user.AddRole(ownerRole);

            // 6. ✅ Auto-approve the role assignment (since it's the owner)
            var userRole = user.UserRoles.FirstOrDefault(ur => ur.RoleId == ownerRole.Id);
            if (userRole is null)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                return Response<string>.FailureResponse(Errors.Exception(
                    new InvalidOperationException("Owner UserRole was not created.")));
            }
            _unitOfWork.Repository<UserRole>().Insert(userRole);

            userRole.Approve();
            userRole.SetUpdate(user.Id);


            // 5. Grant Owner role every ModulePermission for this company's ProductCode.
            //    UNVERIFIED: adjust predicate field name once ModulePermission.cs is confirmed.
            //var modulePermissionRepo = _unitOfWork.Repository<ModulePermission>();
            //var permissions = await modulePermissionRepo.GetListAsync(
            //    predicate: mp => mp.ProductCode == company.ProductCode,
            //    disableTracking: true,
            //    cancellationToken: cancellationToken);

            //if (permissions.Count == 0)
            //{

            //    await _unitOfWork.RollbackAsync(cancellationToken);
            //    return Response<CompanyCreateViewModel>.FailureResponse(Errors.CompanyAlreadyExists); // TODO: dedicated error code
            //}

            //foreach (var permission in permissions)
            //    ownerRole.AddRoleModulePermission(permission);

            // 6. Default subscription — trial
            //Subscription subscription = new(
            //    companyId: company.Id,
            //    productCode: company.ProductCode!,
            //    planName: "Trial",
            //    isTrial: true,
            //    seatLimit: 10
            //    );

            //var subscriptionRepo = _unitOfWork.Repository<Subscription>();
            //subscriptionRepo.Insert(subscription);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync();

            // 7. Invite email — token generated AFTER commit so it's issued only
            //    for a user that's actually persisted. Publish is post-commit,
            //    matching the UserCreatedEvent-after-SaveChanges fix already made
            //    elsewhere in this project.
            //string token = await _userManager.GeneratePasswordResetTokenAsync(user);
            //string encodedToken = HttpUtility.UrlEncode(token);
            //string encodedEmail = HttpUtility.UrlEncode(request.MainUserEmail);

            //var evt = new UserCreatedEvent(
            //    user.FullName!,
            //    user.UserName!,
            //    user.Email!,
            //    _mailConfig.OfficeURL + string.Format(_mailConfig.OfficeNewUserUrl, encodedToken, encodedEmail)
            //    );

            //await _mediator.Publish(evt, cancellationToken);

            return Response<string>.SuccessResponse(
                $"{company.Name} created successfully");
        }
        catch (DbUpdateConcurrencyException cex)
        {
            var conflicts = cex.Entries
        .Select(e => new { Type = e.Entity?.GetType().Name ?? "unknown", e.State })
        .ToList();

            await _unitOfWork.RollbackAsync(cancellationToken);

            _logger.LogError(cex,
                "Concurrency conflict while creating company for {@Request}. Conflicting entries: {@Conflicts}",
                request, conflicts);

            return Response<string>.FailureResponse(Errors.Exception(cex));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "An error occurred creating company for request {@Request}", request);
            return Response<string>.FailureResponse(Errors.Exception(ex));
        }
    }
}
