using UserManagement.Domain.Entities.BaseEntities;

namespace Shared.Infrastructure.Data.Configurations.UserManagement;

public abstract class AuditableEntityConfiguration<T> : IEntityTypeConfiguration<T> where T : AuditableEntity
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        // Configure the relationship to User for EntryBy
        builder.HasOne(e => e.EntryBy)
               .WithMany()                // User does not have a collection of T
               .HasForeignKey(e => e.EntryByUserId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.SetNull);

        // Configure the relationship to User for UpdatedBy
        builder.HasOne(e => e.UpdatedBy)
               .WithMany()
               .HasForeignKey(e => e.UpdatedByUserId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
