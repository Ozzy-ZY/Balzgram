using Microsoft.AspNetCore.Identity;

namespace Domain.Models;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public List<Image> Images { get; set; } = new();
    public List<Message> Messages { get; set; } = new();
    public List<ChatMember> ChatMembers { get; set; } = new();
    public List<RefreshToken> RefreshTokens { get; set; } = new();
}

