namespace UserManagement.Application.Commands.CompanyCommands.CreateCompany;

public class CreateCompnayCommandValidator :
    AbstractValidator<CreateCompanyCommand>
{
    public CreateCompnayCommandValidator()
    {
        RuleFor(x => x.ProductCode)
            .NotEmpty().WithMessage("Product code is required.")
            .MaximumLength(50).WithMessage("Product code cannot exceed 50 characters.");
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Company name is required.")
            .MaximumLength(100).WithMessage("Company name cannot exceed 100 characters.");
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required.")
            .MaximumLength(200).WithMessage("Address cannot exceed 200 characters.");
        RuleFor(x => x.PhoneNo)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format.").Length(9, 15);
        RuleFor(x => x.Pan)
            .NotEmpty().WithMessage("PAN is required.")
            .MaximumLength(10).WithMessage("PAN cannot exceed 10 characters.");
        RuleFor(x => x.RegNo)
            .NotEmpty().WithMessage("Registration number is required.")
            .MaximumLength(20).WithMessage("Registration number cannot exceed 20 characters.");
        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("URL is required.")
            .Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute)).WithMessage("Invalid URL format.");


    }
}
