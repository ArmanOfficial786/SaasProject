//using Microsoft.Extensions.Logging;

//namespace UserManagement.Application.Commands.CompanyCommands.CreateCompany;

//public class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, Response<CompanyCreateViewModel>>
//{
//    private readonly IUnitOfWork _unitOfWork;
//    private readonly IMapper _mapper;
//    private readonly UserManager<User> _userManager;
//    private readonly IMediator _mediator;
//    private readonly ILogger<CreateCompanyCommandHandler> _logger;
//    private readonly MailConfig _mailConfig;

//    public CreateCompanyCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, UserManager<User> userManager, IMediator mediator, ILogger<CreateCompanyCommandHandler> logger, MailConfig mailConfig)
//    {
//        _unitOfWork = unitOfWork;
//        _mapper = mapper;
//        _userManager = userManager;
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

//        var userStatusRepo = _unitOfWork.Repository<UserStatus>();

//        bool isFirstCompany = !await userStatusRepo.GetAnyAsync(cancellationToken: cancellationToken);

//        Company company = new(
//              request.ProductCode,
//              request.Name,
//              request.Email,
//              request.Address,
//              request.PhoneNo,
//              request.Pan,
//              request.RegNo,
//              request.Url
//              );
//        companyRepo.Insert(company);
//        Agent agent = new(
//            name: "Main Agent",
//            address: request.Address,
//            pan: request.Pan,
//            regNo: request.RegNo,
//            isParent: true,
//            referralCode: null!, // Agent.CreateReferralCode() generates one if null is passed through Update/logic; adjust if your ctor doesn't null-coalesce
//            companyId: company.Id
//            );
//        company.AddAgent(agent);
//        User user = new(
//          companyId: company.Id,
//          userName: request.MainUsername,
//          firstName: request.MainUserFirstName,
//          middleName: null,
//          lastName: request.MainUserLastName,
//          email: request.MainUserEmail,
//          contact: request.MainUserContactNo,
//          entryByUserId: null
//          );

//        user.AddToAgent(agent);
//        //company.AddAgent(agent);
//        //companyRepo.Insert(company);

//        await _userManager.CreateAsync(user);
//        await _unitOfWork.SaveChangesAsync(cancellationToken);




//        return Response<CompanyCreateViewModel>.SuccessResponse(_mapper.Map<CompanyCreateViewModel>(company), "Company created successfully");




//    }
//}


using Microsoft.Extensions.Logging;

namespace UserManagement.Application.Commands.CompanyCommands.CreateCompany;

public class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, Response<CompanyCreateViewModel>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;
    private readonly IMediator _mediator;
    private readonly ILogger<CreateCompanyCommandHandler> _logger;
    private readonly MailConfig _mailConfig;

    public CreateCompanyCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, UserManager<User> userManager, IMediator mediator, ILogger<CreateCompanyCommandHandler> logger, MailConfig mailConfig)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _userManager = userManager;
        _mediator = mediator;
        _logger = logger;
        _mailConfig = mailConfig;
    }

    public async Task<Response<CompanyCreateViewModel>> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
    {
        var companyRepo = _unitOfWork.Repository<Company>();

        var dupCompany = await companyRepo.GetSingleOrDefaultAsync(
            predicate: c => c.Name == request.Name || c.Email == request.Email || c.Pan == request.Pan || c.RegNo == request.RegNo,
            disableTracking: true,
            cancellationToken: cancellationToken);

        if (dupCompany != null)
        {
            return Response<CompanyCreateViewModel>.FailureResponse(Errors.CompanyAlreadyExists);
        }

        // 1. Insert Company and SAVE IMMEDIATELY so company.Id is populated
        //    with the real identity value before anything tries to FK against it.
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
        await _unitOfWork.SaveChangesAsync(cancellationToken); // <-- flushes Company, company.Id is now real

        // 2. NOW company.Id is a real, committed value — safe to reference from Agent/User
        Agent agent = new(
            name: "Main Agent",
            address: request.Address,
            pan: request.Pan,
            regNo: request.RegNo,
            isParent: true,
            referralCode: $"{request.RegNo}--{company.Id}",
            companyId: company.Id
            );

        company.AddAgent(agent);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
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
            var message = string.Join("; ", identityResult.Errors.Select(e => e.Description));
            _logger.LogWarning("Failed to create main user for company {CompanyId}: {Errors}", company.Id, message);
            // Company row already exists at this point with no user — decide how you
            // want to handle this (delete the company, mark it inactive, or return as-is).
            return Response<CompanyCreateViewModel>.FailureResponse(Errors.CompanyAlreadyExists); // TODO: replace with a proper error code
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken); // flushes Agent (and anything else pending)

        return Response<CompanyCreateViewModel>.SuccessResponse(
            _mapper.Map<CompanyCreateViewModel>(company),
            "Company created successfully");
    }
}
