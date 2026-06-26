using System.ComponentModel.DataAnnotations;
using UserManagement.Domain.Entities.BaseEntities;

namespace UserManagement.Domain.Entities;

public class Permission : BaseEntity
{
    public Guid CompanyId { get; private set; }
    [Key]
    public int PermissionId { get; set; }
    public string Code { get; set; } = string.Empty;      // e.g., "hrm.employee.view"
    public string Module { get; set; } = string.Empty;     // "HRM", "Accounting"
    public string? Description { get; set; }
}
