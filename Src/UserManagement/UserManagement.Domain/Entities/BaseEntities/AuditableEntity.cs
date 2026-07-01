
using Shared.Domain.Abstraction.Enum;

namespace UserManagement.Domain.Entities.BaseEntities;

public abstract class AuditableEntity : BaseEntity
{
    // Tenant isolation - explicit CompanyId property
    public Guid CompanyId { get; set; }

    // Foreign keys for audit trail
    public Guid? EntryByUserId { get; private set; }
    public User? EntryBy { get; private set; }

    public Guid? UpdatedByUserId { get; private set; }
    public User? UpdatedBy { get; private set; }

    public DateTime EntryDate { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; private set; } = DateTime.UtcNow;
    public DateTime? ToDate { get; private set; }
    public VerificationStatus VerificationStatus { get; private set; } = VerificationStatus.Saved;

    public void SetEntry(User? entryBy)
    {
        EntryBy = entryBy;
        UpdatedDate = DateTime.UtcNow;
    }

    public void SetUpdate(User? updatedBy)
    {
        UpdatedBy = updatedBy;
        UpdatedDate = DateTime.UtcNow;
    }

    public void Submit()
    {
        VerificationStatus = VerificationStatus.Submitted;
    }

    public void Approve()
    {
        VerificationStatus = VerificationStatus.Approved;
    }
    public void Reject()
    {
        VerificationStatus = VerificationStatus.Rejected;
    }

    public bool IsTerminated => ToDate != null;
    public bool IsVerified => VerificationStatus == VerificationStatus.Approved;
    public bool IsRejected => VerificationStatus == VerificationStatus.Rejected;
    public bool IsUnapproved => VerificationStatus == VerificationStatus.Submitted;

    public bool ValidOnDate(DateTime date)
    {
        return ValidOnDate(DateOnly.FromDateTime(date));
    }

    public bool ValidOnDate(DateOnly date)
    {
        if (!IsVerified)
            return false;
        return date >= DateOnly.FromDateTime(EntryDate) && (ToDate == null || date <= DateOnly.FromDateTime(ToDate.Value));
    }
}
