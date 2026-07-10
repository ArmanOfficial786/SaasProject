//namespace Shared.Infrastructure.Data.Configurations.UserManagement;

//internal class CompanRoleConfiguration : IEntityTypeConfiguration<CompanyRole>
//{
//    public void Configure(EntityTypeBuilder<CompanyRole> builder)
//    {
//        _ = builder.ToTable("company_roles", Schemas.UserManagement);

//        _ = builder
//            .HasOne(x => x.Role)
//            .WithMany()
//            .OnDelete(DeleteBehavior.Cascade);
//    }
//}
