namespace UserManagement.Domain.Entities;

public class UserStatus
{
    public int Id { get; private set; }
    public DateTime FromDate { get; private set; }
    public DateTime? ToDate { get; private set; }

    public bool IsActive => !ToDate.HasValue || ToDate.Value > DateTime.UtcNow;

    public string? Remarks { get; private set; }

    public UserStatus(string? remarks = null)
    {
        Remarks = remarks ?? "User created";
    }

    public void Terminate(string? remarks = null)
    {
        ToDate = DateTime.UtcNow;
        Remarks = remarks ?? "User terminated";
    }

    private UserStatus() { } // For EF Core

}
