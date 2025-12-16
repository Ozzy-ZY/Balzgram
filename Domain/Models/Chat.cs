using System.ComponentModel.DataAnnotations;

namespace Domain.Models;

public enum ChatType
{
    Direct = 1,
    Group = 2
}

public class Chat
{
    public Guid Id { get; set; }
    public ChatType Type { get; set; }
    [MaxLength(200)]
    public string? Name { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string CreatedByUserId { get; set; } = string.Empty;

    public ICollection<ChatMember> Members { get; set; } = new List<ChatMember>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}

