using Shared.Application.SeedData;

namespace Hrm.Api.Extensions;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var initialiser = scope.ServiceProvider.GetRequiredService<DbInitializer>();
        await initialiser.SeedAsync();
    }
}
