namespace Domain.Entitites.Base;

public abstract class AuditableBaseEntity<T>
{
    public virtual T Id { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    public DateTime? DeletedAt { get; set; }
}