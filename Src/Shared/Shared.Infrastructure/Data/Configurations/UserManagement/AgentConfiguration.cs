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

        // Seed data
        //var seedAgents = new List<Agent>();
        //builder.HasData(seedAgents);

        // ✅ Tenant: Agent belongs to a Company
        _ = builder.HasOne(x => x.Company)
            .WithMany(x => x.Agents)
            .HasForeignKey("CompanyId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
