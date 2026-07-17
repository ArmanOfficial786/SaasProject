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

        // ✅ NEW: explicit User side — this was missing
        builder.HasOne<User>()
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

    }
}
