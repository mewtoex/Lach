namespace WhatsAppService.Entities;

public class WhatsAppContactEntity
{
    public int Id { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Email { get; set; }
    public Guid? CustomerId { get; set; }
    public DateTime LastInteraction { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
} 