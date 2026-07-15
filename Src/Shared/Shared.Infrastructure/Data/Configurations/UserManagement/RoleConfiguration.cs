namespace Shared.Infrastructure.Data.Configurations.UserManagement;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles", Schemas.UserManagement);

        builder.HasKey(r => r.Id);

        // Explicit CompanyId property for tenant isolation
        builder.Property(r => r.CompanyId).IsRequired();
        builder.Property(r => r.Desc).HasMaxLength(500);

        // Seed data
        //var seedRoles = new List<Role>
        //{
        //    SeedRole.Admin,
        //    SeedRole.Manager,
        //    SeedRole.User,
        //};
        //builder.HasData(seedRoles);

        // ✅ Relationship: Role → Company (Many‐to‐One)
        builder.HasOne(r => r.Company)
               .WithMany(c => c.Roles)
               .HasForeignKey(r => r.CompanyId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.EntryBy)
            .WithMany()
            .HasForeignKey("EntryByUserId")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint per tenant
        builder.HasIndex(r => new { r.CompanyId, r.NormalizedName }).IsUnique();

        builder.Navigation(r => r.RoleModulePermissions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class SeedRole
{
    public static Role Admin = new(
        companyId: 1,
        name: "Admin",
        desc: "Administrator with full access"
    )
    {
        Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
        NormalizedName = "ADMIN",
        ConcurrencyStamp = Guid.NewGuid().ToString()
    };

    public static Role Manager = new(
        companyId: 1,
        name: "Manager",
        desc: "Manager with operational access"
    )
    {
        Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
        NormalizedName = "MANAGER",
        ConcurrencyStamp = Guid.NewGuid().ToString()
    };

    public static Role User = new(
        companyId: 1,
        name: "User",
        desc: "Regular user with limited access"
    )
    {
        Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
        NormalizedName = "USER",
        ConcurrencyStamp = Guid.NewGuid().ToString()
    };
}
