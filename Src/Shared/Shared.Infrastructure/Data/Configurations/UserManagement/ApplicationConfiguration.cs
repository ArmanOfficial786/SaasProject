using UserManagement.Domain.Enum;
using App = Security.Domain.Entities.Application;
namespace Shared.Infrastructure.Data.Configurations.SecurityConfigurations;

public class ApplicationConfiguration : IEntityTypeConfiguration<App>
{
    public void Configure(EntityTypeBuilder<App> builder)
    {
        _ = builder.ToTable("applications", Schemas.UserManagement);

        var seedApplications = new List<App>
        {
            SeedApplication.UserManagement,
            //add future application
        };

        _ = builder.HasData(seedApplications);
    }
}

public class SeedApplication
{
    public static App UserManagement = new(Guid.Parse("89de1083-5d8b-401c-8914-7f6cc1363fdf"), "Usermanagement", "Usermanagement", ApplicationEnum.Usermanagement);
    //add futrure application 
}
