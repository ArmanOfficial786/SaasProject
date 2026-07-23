namespace Shared.Infrastructure.Data.Configurations.SecurityConfigurations;

public class AgentConfiguration : IEntityTypeConfiguration<Agent>
{
    public void Configure(EntityTypeBuilder<Agent> builder)
    {
        _ = builder.ToTable("agents", Schemas.UserManagement);

        _ = builder.HasKey(a => a.Id);

        _ = builder.Property(x => x.CompanyId).IsRequired();

        _ = builder.HasMany(x => x.RolesForUser)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);
        _ = builder.HasIndex(x => x.ReferralCode)
            .IsUnique();



        // ✅ Tenant: Agent belongs to a Company
        _ = builder.HasOne(x => x.Company)
            .WithMany(x => x.Agents)
            .HasForeignKey("CompanyId")
            .OnDelete(DeleteBehavior.Cascade);
        // Seed data
        var seedAgents = new List<Agent>
        {
            SeedAgent.DefaultAgent
        };
        builder.HasData(seedAgents);

    }
}


public static class SeedAgent
{
    public static Agent DefaultAgent = new(
        name: "Head Office",
        address: "Kathmandu, Nepal",
        pan: "123456789",
        regNo: "REG-001",
        isParent: true,
        referralCode: "REG-001--1",
        companyId: 1
    )
    {
        Id = Guid.Parse("20000000-0000-0000-0000-000000000001")
    };
}
