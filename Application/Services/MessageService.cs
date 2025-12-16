using Application.DTOs.Chat;
using Application.Interfaces;
using Domain.Models;

namespace Application.Services;

public class MessageService : IMessageService
{
    private readonly IChatRepository _chatRepository;
    private readonly IMessageRepository _messageRepository;

    public MessageService(IChatRepository chatRepository, IMessageRepository messageRepository)
    {
        _chatRepository = chatRepository;
        _messageRepository = messageRepository;
    }

    public async Task<MessageDto> SendMessageAsync(string currentUserId, Guid chatId, string content, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Message content is required", nameof(content));
        }

        var isMember = await _chatRepository.IsUserInChatAsync(chatId, currentUserId, cancellationToken);
        if (!isMember)
        {
            throw new UnauthorizedAccessException("You are not a member of this chat.");
        }

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            SenderId = currentUserId,
            Content = content,
            SentAtUtc = DateTime.UtcNow
        };

        await _messageRepository.AddAsync(message, cancellationToken);
        await _messageRepository.SaveChangesAsync(cancellationToken);

        return new MessageDto
        {
            Id = message.Id,
            ChatId = message.ChatId,
            SenderUserName = string.Empty,
            Content = message.Content,
            SentAtUtc = message.SentAtUtc
        };
    }

    public async Task<PagedResultDto<MessageDto>> GetMessagesAsync(string currentUserId, Guid chatId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 50;

        var isMember = await _chatRepository.IsUserInChatAsync(chatId, currentUserId, cancellationToken);
        if (!isMember)
        {
            throw new UnauthorizedAccessException("You are not a member of this chat.");
        }

        var (items, totalCount) = await _messageRepository.GetMessagesPagedAsync(chatId, pageNumber, pageSize, cancellationToken);

        var messageDtos = items
            .OrderBy(m => m.SentAtUtc)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                ChatId = m.ChatId,
                SenderUserName = string.Empty,
                Content = m.Content,
                SentAtUtc = m.SentAtUtc
            })
            .ToList();

        return new PagedResultDto<MessageDto>
        {
            Items = messageDtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}

