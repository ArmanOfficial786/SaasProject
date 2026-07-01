namespace Shared.Infrastructure.Data.Configurations.SecurityConfigurations;

public class AgentConfiguration : IEntityTypeConfiguration<Agent>
{
    public void Configure(EntityTypeBuilder<Agent> builder)
    {
        _ = builder.ToTable("agents", Schemas.UserManagement);

        // Configure CompanyId for tenant isolation
        _ = builder.Property(x => x.CompanyId).IsRequired();

        _ = builder.HasOne(x => x.Role)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        _ = builder.HasMany(x => x.RolesForUser)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);
        _ = builder.HasIndex(x => x.ReferralCode)
            .IsUnique();

        _ = builder.HasOne(x => x.Company)
            .WithMany(x => x.Agents)
            .HasForeignKey("CompanyId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
