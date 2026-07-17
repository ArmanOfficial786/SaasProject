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


    }
}
