namespace Domain.Common;

public abstract class BaseEntity : IBaseAuditableEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
}
