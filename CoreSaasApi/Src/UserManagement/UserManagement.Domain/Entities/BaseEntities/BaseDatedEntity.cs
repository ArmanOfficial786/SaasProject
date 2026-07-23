namespace UserManagement.Domain.Entities.BaseEntities;

public abstract class BaseDatedEntity() : BaseEntity
{
    public DateTime FromDate { get; set; } = DateTime.UtcNow;

    public DateOnly? ToDate { get; private set; }

    public void Terminate()
    {
        ToDate = DateOnly.FromDateTime(DateTime.UtcNow);
    }
}
