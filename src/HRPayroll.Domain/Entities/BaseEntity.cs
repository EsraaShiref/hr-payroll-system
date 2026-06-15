namespace HRPayroll.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; internal set; }
    public string CreatedBy { get; internal set; } = string.Empty;
    public DateTime ModifiedAt { get; internal set; }
    public string ModifiedBy { get; internal set; } = string.Empty;
    public bool IsDeleted { get; protected set; }
    public DateTime? DeletedAt { get; protected set; }
    public string? DeletedBy { get; protected set; }

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        ModifiedAt = DateTime.UtcNow;
    }

    public void SetAuditFields(string createdBy)
    {
        CreatedBy = createdBy;
        ModifiedBy = createdBy;
    }

    public void MarkModified(string modifiedBy)
    {
        ModifiedAt = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
    }

    public void MarkDeleted(string deletedBy)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
        MarkModified(deletedBy);
    }
}
