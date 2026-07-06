using Microsoft.Extensions.DependencyInjection;

namespace UserManagement.Application;

/// <summary>
/// MediatR handlers, FluentValidation validators, and AutoMapper profiles in this
/// assembly are already picked up automatically by Shared.Application's and
/// Shared.Infrastructure's AppDomain-wide scans (see AddSharedApplication and
/// AddSharedInfrastructure). Re-registering AddMediatR / AddValidatorsFromAssembly
/// here would create duplicate handler and validator registrations — e.g. a second
/// IRequestHandler for CreateUserCommand, or a validator that runs twice and doubles
/// up validation errors in the response.
///
/// This method is kept as an explicit, readable step in Program.cs and as a home
/// for any future UserManagement-specific registrations that should NOT be scanned
/// globally (e.g. a service that should only exist in this module).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        //Register application level services here if any in future
        return services;
    }
}
