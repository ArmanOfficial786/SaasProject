namespace Shared.Infrastructure.Data.Configurations.UserManagement;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("companies");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasColumnName("CompanyId")
            .ValueGeneratedNever(); // BaseEntity already sets Id = Guid.NewGuid()

        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Email).HasMaxLength(256).IsRequired();
        builder.Property(c => c.Address).HasMaxLength(500);
        builder.Property(c => c.PhoneNo).HasMaxLength(20);
        builder.Property(c => c.Pan).HasMaxLength(50);
        builder.Property(c => c.RegNo).HasMaxLength(50);
        builder.Property(c => c.Url).HasMaxLength(256);

        builder.HasIndex(c => c.Pan).IsUnique();
        builder.HasIndex(c => c.RegNo).IsUnique();

        builder.HasMany(c => c.Users)
            .WithOne()
            .HasForeignKey(u => u.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(c => c.Users).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(c => c.RolesForUser).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
