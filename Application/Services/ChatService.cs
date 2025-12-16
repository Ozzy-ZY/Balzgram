using Application.DTOs.Chat;
using Application.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class ChatService : IChatService
{
    private readonly IChatRepository _chatRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public ChatService(IChatRepository chatRepository, UserManager<ApplicationUser> userManager)
    {
        _chatRepository = chatRepository;
        _userManager = userManager;
    }

    public async Task<ChatDto> CreatePrivateChatAsync(string currentUserId, string targetUserName, CancellationToken cancellationToken = default)
    {
        var currentUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == currentUserId, cancellationToken)
                          ?? throw new InvalidOperationException("Current user not found");

        var targetUser = await _userManager.FindByNameAsync(targetUserName)
                         ?? throw new KeyNotFoundException("Target user not found");

        if (targetUser.Id == currentUserId)
        {
            throw new InvalidOperationException("Cannot create a private chat with yourself.");
        }

        var existingChat = await _chatRepository.GetPrivateChatBetweenUsersAsync(currentUserId, targetUser.Id, cancellationToken);
        if (existingChat != null)
        {
            return MapToChatDto(existingChat);
        }

        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            Type = ChatType.Direct,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedByUserId = currentUserId
        };

        chat.Members.Add(new ChatMember
        {
            ChatId = chat.Id,
            UserId = currentUserId,
            IsOwner = true,
            IsAdmin = true,
            JoinedAtUtc = DateTime.UtcNow
        });

        chat.Members.Add(new ChatMember
        {
            ChatId = chat.Id,
            UserId = targetUser.Id,
            JoinedAtUtc = DateTime.UtcNow
        });

        await _chatRepository.AddAsync(chat, cancellationToken);
        await _chatRepository.SaveChangesAsync(cancellationToken);

        return MapToChatDto(chat);
    }

    public async Task<ChatDto> CreateGroupChatAsync(string currentUserId, string name, IEnumerable<string> initialMemberUserNames, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Group name is required", nameof(name));
        }

        var creator = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == currentUserId, cancellationToken)
                      ?? throw new InvalidOperationException("Current user not found");

        var usernames = initialMemberUserNames?.Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? new List<string>();

        var members = new List<ApplicationUser>();
        foreach (var username in usernames)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user != null)
            {
                members.Add(user);
            }
        }

        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            Type = ChatType.Group,
            Name = name,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedByUserId = currentUserId
        };

        chat.Members.Add(new ChatMember
        {
            ChatId = chat.Id,
            UserId = currentUserId,
            IsOwner = true,
            IsAdmin = true,
            JoinedAtUtc = DateTime.UtcNow
        });

        foreach (var member in members.Where(m => m.Id != currentUserId))
        {
            chat.Members.Add(new ChatMember
            {
                ChatId = chat.Id,
                UserId = member.Id,
                JoinedAtUtc = DateTime.UtcNow
            });
        }

        await _chatRepository.AddAsync(chat, cancellationToken);
        await _chatRepository.SaveChangesAsync(cancellationToken);

        return MapToChatDto(chat);
    }

    public async Task AddUserToGroupAsync(string currentUserId, Guid chatId, string targetUserName, CancellationToken cancellationToken = default)
    {
        var chat = await _chatRepository.GetByIdAsync(chatId, cancellationToken) ?? throw new KeyNotFoundException("Chat not found");

        if (chat.Type != ChatType.Group)
        {
            throw new InvalidOperationException("Cannot add members to a direct chat.");
        }

        if (!chat.Members.Any(m => m.UserId == currentUserId && m.LeftAtUtc == null))
        {
            throw new UnauthorizedAccessException("You are not a member of this chat.");
        }

        var targetUser = await _userManager.FindByNameAsync(targetUserName)
                         ?? throw new KeyNotFoundException("Target user not found");

        if (chat.Members.Any(m => m.UserId == targetUser.Id && m.LeftAtUtc == null))
        {
            return;
        }

        chat.Members.Add(new ChatMember
        {
            ChatId = chat.Id,
            UserId = targetUser.Id,
            JoinedAtUtc = DateTime.UtcNow
        });

        await _chatRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveUserFromGroupAsync(string currentUserId, Guid chatId, string targetUserName, CancellationToken cancellationToken = default)
    {
        var chat = await _chatRepository.GetByIdAsync(chatId, cancellationToken) ?? throw new KeyNotFoundException("Chat not found");

        if (chat.Type != ChatType.Group)
        {
            throw new InvalidOperationException("Cannot remove members from a direct chat.");
        }

        if (!chat.Members.Any(m => m.UserId == currentUserId && m.LeftAtUtc == null))
        {
            throw new UnauthorizedAccessException("You are not a member of this chat.");
        }

        var targetUser = await _userManager.FindByNameAsync(targetUserName)
                         ?? throw new KeyNotFoundException("Target user not found");

        var membership = chat.Members.FirstOrDefault(m => m.UserId == targetUser.Id && m.LeftAtUtc == null);
        if (membership == null)
        {
            return;
        }

        membership.LeftAtUtc = DateTime.UtcNow;

        await _chatRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task LeaveGroupChatAsync(string currentUserId, Guid chatId, CancellationToken cancellationToken = default)
    {
        var chat = await _chatRepository.GetByIdAsync(chatId, cancellationToken) ?? throw new KeyNotFoundException("Chat not found");

        if (chat.Type != ChatType.Group)
        {
            throw new InvalidOperationException("Cannot leave a direct chat.");
        }

        var membership = chat.Members.FirstOrDefault(m => m.UserId == currentUserId && m.LeftAtUtc == null)
                         ?? throw new UnauthorizedAccessException("You are not a member of this chat.");

        membership.LeftAtUtc = DateTime.UtcNow;

        await _chatRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ChatDto>> GetUserChatsAsync(string currentUserId, CancellationToken cancellationToken = default)
    {
        var chats = await _chatRepository.GetUserChatsAsync(currentUserId, cancellationToken);
        return chats.Select(MapToChatDto).ToList();
    }

    public Task<bool> IsUserInChatAsync(Guid chatId, string userId, CancellationToken cancellationToken = default)
    {
        return _chatRepository.IsUserInChatAsync(chatId, userId, cancellationToken);
    }

    private static ChatDto MapToChatDto(Chat chat)
    {
        var lastMessage = chat.Messages
            .OrderByDescending(m => m.SentAtUtc)
            .FirstOrDefault();

        return new ChatDto
        {
            Id = chat.Id,
            IsGroup = chat.Type == ChatType.Group,
            Name = chat.Name,
            LastMessagePreview = lastMessage?.Content,
            LastMessageAtUtc = lastMessage?.SentAtUtc,
            MemberCount = chat.Members.Count(m => m.LeftAtUtc == null)
        };
    }
}
