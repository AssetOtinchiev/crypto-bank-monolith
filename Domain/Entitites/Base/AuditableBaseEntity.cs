namespace Domain.Entitites.Base;

public abstract class AuditableBaseEntity<T>
{
    public virtual T Id { get; set; }
    
    
    public DateTime CreatedAt { get; set; } = DateTime.Now.ToUniversalTime();

    public DateTime UpdatedAt { get; set; } = DateTime.Now.ToUniversalTime();
    
    public DateTime? DeletedAt { get; set; }
}