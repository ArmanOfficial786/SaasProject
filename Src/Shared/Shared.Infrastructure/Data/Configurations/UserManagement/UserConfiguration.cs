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
        builder.Property(u => u.CompanyId).IsRequired();

        // FIX #2: Map EntryByUserId scalar to FK column, no navigation
        builder.Property(u => u.EntryByUserId).HasColumnName("EntryByUserId");
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(u => u.EntryByUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(u => new { u.CompanyId, u.NormalizedEmail }).IsUnique();

        // Relationship to Company
        builder.HasOne(u => u.Company)
            .WithMany(c => c.Users)
            .HasForeignKey(u => u.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);



        // Note: Users are NOT seeded here via HasData — password hashing
        // needs UserManager at runtime. See DbInitializer.SeedAsync().

    }
}
