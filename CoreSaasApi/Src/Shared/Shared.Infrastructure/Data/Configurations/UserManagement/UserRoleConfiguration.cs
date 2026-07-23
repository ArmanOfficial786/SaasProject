//namespace Shared.Infrastructure.Data.Configurations.UserManagement;

//public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
//{
//    public void Configure(EntityTypeBuilder<UserRole> builder)
//    {
//        builder.ToTable("user_roles", Schemas.UserManagement);
//        builder.HasKey(ur => ur.Id);

//        builder.Property(ur => ur.UserId).IsRequired();
//        builder.Property(ur => ur.RoleId).IsRequired();

//        builder.Property(ur => ur.CompanyId).IsRequired(false);

//        // ✅ NEW: explicit User side — this was missing
//        builder.HasOne<User>()
//            .WithMany(u => u.UserRoles)
//            .HasForeignKey(ur => ur.UserId)
//            .OnDelete(DeleteBehavior.Cascade);

//        // ✅ Relationship: UserRole → Role
//        builder.HasOne(ur => ur.Role)
//            .WithMany()
//            .HasForeignKey(ur => ur.RoleId)
//            .OnDelete(DeleteBehavior.Cascade);

//        // ✅ Indexes
//        builder.HasIndex(ur => new { ur.UserId, ur.CompanyId });


//        // ✅ Index for Super Admin roles (CompanyId = NULL)
//        builder.HasIndex(ur => new { ur.UserId, ur.RoleId });

//        //.HasFilter("[company_id] IS NULL AND [is_active] = 1");

//        // ✅ Unique active assignments
//        builder.HasIndex(ur => new { ur.UserId, ur.RoleId, ur.CompanyId })
//               .IsUnique();
//        //.HasFilter("[to_date] IS NULL AND [is_active] = 1");

//        // Seed default user-role assignment
//        var seedUserRoles = new List<UserRole>
//        {
//            SeedUserRole.DefaultUserOwnerRole
//        };
//        builder.HasData(seedUserRoles);

//    }
//}


//public static class SeedUserRole
//{
//    public static UserRole DefaultUserOwnerRole = new(
//        userId: Guid.Parse("30000000-0000-0000-0000-000000000001"), // Default User ID
//        roleId: Guid.Parse("10000000-0000-0000-0000-000000000001")   // Owner Role ID
//    )
//    {
//        IsActive = true,
//        IsApproved = true,
//        IsDeleted = false,
//        ApprovedDate = DateTime.UtcNow,
//        FromDate = DateTime.UtcNow
//    };
//}








namespace Shared.Infrastructure.Data.Configurations.UserManagement;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles", Schemas.UserManagement);
        builder.HasKey(ur => ur.Id);

        builder.Property(ur => ur.UserId).IsRequired();
        builder.Property(ur => ur.RoleId).IsRequired();
        builder.Property(ur => ur.CompanyId).IsRequired(false);

        // ✅ Relationship: UserRole → User (uses the real navigation property)
        builder.HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ✅ Relationship: UserRole → Role
        builder.HasOne(ur => ur.Role)
            .WithMany()
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        // ✅ Indexes
        builder.HasIndex(ur => new { ur.UserId, ur.CompanyId });

        // ✅ Index for Super Admin roles (CompanyId = NULL)
        builder.HasIndex(ur => new { ur.UserId, ur.RoleId });
        //.HasFilter("[company_id] IS NULL AND [is_active] = 1");

        // ✅ Unique active assignments
        builder.HasIndex(ur => new { ur.UserId, ur.RoleId, ur.CompanyId })
               .IsUnique();
        //.HasFilter("[to_date] IS NULL AND [is_active] = 1");

        // Seed default user-role assignment
        var seedUserRoles = new List<UserRole>
        {
            SeedUserRole.DefaultUserOwnerRole,
            SeedUserRole.DefaultUserAdminRole
        };
        builder.HasData(seedUserRoles);
    }
}

public static class SeedUserRole
{
    private static readonly DateTime SeedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static UserRole DefaultUserOwnerRole = new(
        userId: Guid.Parse("30000000-0000-0000-0000-000000000001"), // Default User ID
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000001"), // Owner Role ID
        companyId: 1
    );

    // Optional: Add more role assignments
    public static UserRole DefaultUserAdminRole = new(
        userId: Guid.Parse("30000000-0000-0000-0000-000000000001"),
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000002"), // Admin Role ID
        companyId: 1
    );
}
