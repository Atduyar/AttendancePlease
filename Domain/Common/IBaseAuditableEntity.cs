namespace Domain.Common;

public interface IBaseAuditableEntity
{
    DateTime CreatedAt { get; set; }
}
