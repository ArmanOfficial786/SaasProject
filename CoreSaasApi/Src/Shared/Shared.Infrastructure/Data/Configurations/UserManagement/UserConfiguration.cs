namespace Shared.Infrastructure.Data.Configurations.UserManagement;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", Schemas.UserManagement);
        builder.HasKey(u => u.Id);
        builder.Property(u => u.FirstName).HasMaxLength(30);
        builder.Property(u => u.MiddleName).HasMaxLength(30);
        builder.Property(u => u.LastName).HasMaxLength(30);
        builder.Property(u => u.Email).HasMaxLength(256);
        builder.Property(u => u.Contact).HasMaxLength(256);
        builder.Property(u => u.CompanyId).IsRequired(false);

        // FIX #2: Map EntryByUserId scalar to FK column, no navigation

        // ✅ Relationship: User → Company
        builder.HasOne(u => u.Company)
               .WithMany(c => c.Users)
               .HasForeignKey(u => u.CompanyId)
               .OnDelete(DeleteBehavior.Restrict);
        // ✅ Self-reference for EntryBy
        builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(u => u.EntryByUserId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Restrict);


        // ✅ Unique constraints per company
        builder.HasIndex(u => new { u.CompanyId, u.NormalizedEmail }).IsUnique();

        // UserConfiguration.cs — add at the end of Configure(), before the closing brace
        builder.Navigation(u => u.UserRoles).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(u => u.UserStatuses).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(u => u.AgentUsers).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(u => u.UserModulePermissions).UsePropertyAccessMode(PropertyAccessMode.Field);

        // ✅ Seed default user - using anonymous type to avoid navigation issues
        builder.HasData(
            new
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000001"),
                CompanyId = 1,
                UserName = "admin.arsuhrm",
                NormalizedUserName = "ADMIN.ARSUHRM",
                FirstName = "Arman",
                MiddleName = (string?)null,
                LastName = "Shrestha",
                Email = "admin@arsuhrm.com",
                NormalizedEmail = "ADMIN@ARSUHRM.COM",
                Contact = "9800000001",
                EntryByUserId = (Guid?)null,
                EntryDate = DateTime.UtcNow,

                // Custom required properties on User — these were missing
                // and are what dotnet-ef is complaining about (and would
                // complain about next, one at a time, without this)
                IsEmailConfirmed = false,
                FailedLoginAttempts = 0,
                LockedUntil = (DateTime?)null,

                // IdentityUser properties
                EmailConfirmed = false,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            }
        );


    }
}


//public static class SeedUser
//{
//    public static User DefaultUser = new(
//        companyId: 1, // Reference to default company
//        userName: "admin.arsuhrm",
//        firstName: "Arman",
//        middleName: null,
//        lastName: "Shrestha",
//        email: "admin@arsuhrm.com",
//        contact: "9800000001",
//        entryByUserId: null
//    )
//    {
//        Id = Guid.Parse("30000000-0000-0000-0000-000000000001"),
//        NormalizedUserName = "ADMIN.ARSUHRM",
//        NormalizedEmail = "ADMIN@ARSUHRM.COM",
//        ConcurrencyStamp = Guid.NewGuid().ToString(),
//        SecurityStamp = Guid.NewGuid().ToString()
//    };
//}
