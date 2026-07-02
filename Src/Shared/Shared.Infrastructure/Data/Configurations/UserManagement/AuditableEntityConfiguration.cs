using UserManagement.Domain.Entities.BaseEntities;

namespace Shared.Infrastructure.Data.Configurations.UserManagement;

public abstract class AuditableEntityConfiguration<T> : IEntityTypeConfiguration<T> where T : AuditableEntity
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        // Configure explicit CompanyId property for tenant isolation
        builder.Property(e => e.CompanyId).IsRequired();

        // FIX #2: EntryByUserId and UpdatedByUserId are scalar FKs, not navigation properties.
        // No explicit HasOne relationships needed; EF will infer from the FK properties.
        builder.Property(e => e.EntryByUserId).IsRequired(false);
        builder.Property(e => e.UpdatedByUserId).IsRequired(false);
    }
}
