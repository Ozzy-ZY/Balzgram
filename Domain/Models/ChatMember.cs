namespace Domain.Models;

public class ChatMember
{
    public Guid ChatId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public bool IsOwner { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsMuted { get; set; }
    public DateTime JoinedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LeftAtUtc { get; set; }

    public Chat Chat { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}

