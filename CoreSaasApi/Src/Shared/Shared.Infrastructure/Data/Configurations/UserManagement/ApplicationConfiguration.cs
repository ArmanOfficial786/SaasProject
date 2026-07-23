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
    public static App UserManagement = new(
      id: Guid.Parse("89de1083-5d8b-401c-8914-7f6cc1363fdf"),
      name: "User Management",
      desc: "Identity & Access Management for the SaaS platform",
      code: ApplicationEnum.Usermanagement
    );

    // public static App HRM = new(
    //     id: Guid.Parse("b2c3d4e5-f6a7-4b5c-9d0e-1f2a3b4c5d6e"),
    //     name: "HRM",
    //     desc: "Human Resource Management",
    //     code: ApplicationEnum.HRM
    // );
}
