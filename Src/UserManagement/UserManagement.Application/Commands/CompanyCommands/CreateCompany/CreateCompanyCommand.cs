namespace UserManagement.Application.Commands.CompanyCommands.CreateCompany;

public record CreateCompanyCommand
    (
    string ProductCode,
    string Name,
    string Email,
    string Address,
    string PhoneNo,
    string Pan,
    string RegNo,
    string Url,
    string MainUsername,
    string MainUserFirstName,
    string MainUserLastName,
    string MainUserEmail,
    string MainUserContactNo
    ) : IRequest<Response<CompanyCreateViewModel>>;

