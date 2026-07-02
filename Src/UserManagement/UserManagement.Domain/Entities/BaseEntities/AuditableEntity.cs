using Shared.Domain.Abstraction.Enum;

namespace UserManagement.Domain.Entities.BaseEntities;

// FIX #2: removed "public User? EntryBy" navigation property.
// Stores scalar FK instead. Base class is decoupled from User entity.
public abstract class AuditableEntity : BaseEntity
{
    // Tenant isolation
    public int CompanyId { get; set; }

    // FIX #2: Scalar FK, not navigation — eliminates circular dependency
    public Guid? EntryByUserId { get; private set; }
    public Guid? UpdatedByUserId { get; private set; }

    public DateTime EntryDate { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; private set; }
    public DateTime? ToDate { get; private set; }
    public VerificationStatus VerificationStatus { get; private set; } = VerificationStatus.Saved;

    public void SetEntry(Guid? entryByUserId)
    {
        EntryByUserId = entryByUserId;
        UpdatedDate = DateTime.UtcNow;
    }

    public void SetUpdate(Guid? updatedByUserId)
    {
        UpdatedByUserId = updatedByUserId;
        UpdatedDate = DateTime.UtcNow;
    }

    public void Submit() => VerificationStatus = VerificationStatus.Submitted;
    public void Approve() => VerificationStatus = VerificationStatus.Approved;
    public void Reject() => VerificationStatus = VerificationStatus.Rejected;

    protected void SetTerminationDate(DateTime? date = null)
        => ToDate = date ?? DateTime.UtcNow;

    public bool IsTerminated => ToDate is not null;
    public bool IsVerified => VerificationStatus == VerificationStatus.Approved;
    public bool IsRejected => VerificationStatus == VerificationStatus.Rejected;
    public bool IsUnapproved => VerificationStatus == VerificationStatus.Submitted;

    public bool ValidOnDate(DateTime date)
        => ValidOnDate(DateOnly.FromDateTime(date));

    public bool ValidOnDate(DateOnly date)
    {
        if (!IsVerified) return false;
        return date >= DateOnly.FromDateTime(EntryDate) &&
               (ToDate is null || date <= DateOnly.FromDateTime(ToDate.Value));
    }
}
