namespace Contracts.Dtos.Base;

public class AuditableBaseEntityDto
{
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    public DateTime? DeletedAt { get; set; }
}