using System.ComponentModel.DataAnnotations;

namespace Domain.Models;

public class Message
{
    public Guid Id { get; set; }
    public Guid ChatId { get; set; }
    public string SenderId { get; set; } = string.Empty;
    [MaxLength(4000)]
    public string Content { get; set; } = string.Empty;
    public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }

    public Chat Chat { get; set; } = null!;
    public ApplicationUser Sender { get; set; } = null!;
}

