namespace Shared.Infrastructure.Data.Configurations.UserManagement;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("companies", Schemas.UserManagement);

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Email).HasMaxLength(256).IsRequired();
        builder.Property(c => c.Address).HasMaxLength(500).IsRequired();
        builder.Property(c => c.PhoneNo).HasMaxLength(20).IsRequired();
        builder.Property(c => c.Pan).HasMaxLength(50).IsRequired();
        builder.Property(c => c.RegNo).HasMaxLength(50).IsRequired();
        builder.Property(c => c.Url).HasMaxLength(256);

        builder.HasIndex(c => c.Pan).IsUnique();
        builder.HasIndex(c => c.RegNo).IsUnique();


        builder.HasMany(c => c.RolesForAgent)
               .WithOne()
               .OnDelete(DeleteBehavior.Cascade);

        // ✅ Relationship: Company → Roles (One-to-Many)
        builder.HasMany(c => c.Roles)
               .WithOne(r => r.Company)
               .HasForeignKey(r => r.CompanyId)
               .OnDelete(DeleteBehavior.Restrict);

        // ✅ Relationship: Company → Users (One-to-Many)
        builder.HasMany(c => c.Users)
               .WithOne(u => u.Company)
               .HasForeignKey(u => u.CompanyId)
               .OnDelete(DeleteBehavior.Restrict);

        // ✅ Relationship: Company → Agents (One-to-Many)
        builder.HasMany(c => c.Agents)
               .WithOne(a => a.Company)
               .HasForeignKey(a => a.CompanyId)
               .OnDelete(DeleteBehavior.Cascade);

        // Seed data for default company
        builder.HasData(SeedCompany.DefaultCompany);

    }
}

public static class SeedCompany
{
    public static Company DefaultCompany = Company.CreateSeed(
        id: 1,
        productCode: "HRM",
        name: "ArsuHrm Solutions Pvt. Ltd.",
        email: "info@arsuhrm.com",
        address: "Kathmandu, Nepal",
        phoneNo: "9829967841",
        pan: "123456789",
        regNo: "REG-001",
        url: "https://arsuhrm.com"
    );
}


