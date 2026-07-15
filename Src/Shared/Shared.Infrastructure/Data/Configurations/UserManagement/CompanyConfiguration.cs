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

        //// Seed data - use anonymous type with Id property to set key
        //builder.HasData(
        //    new
        //    {
        //        Id = 1,
        //        Name = "Arman Software Solutions",
        //        Email = "info@armansoftware.com",
        //        Address = "Kathmandu, Nepal",
        //        PhoneNo = "+977-1-4000000",
        //        Pan = "600000000",
        //        RegNo = "120000",
        //        Url = "https://armansoftware.com",
        //        ProductCode = (string?)null
        //    }
        //);

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


    }
}


