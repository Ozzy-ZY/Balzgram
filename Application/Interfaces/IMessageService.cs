using Application.DTOs.Chat;

namespace Application.Interfaces;

public interface IMessageService
{
    Task<MessageDto> SendMessageAsync(string currentUserId, Guid chatId, string content, CancellationToken cancellationToken = default);

    Task<PagedResultDto<MessageDto>> GetMessagesAsync(string currentUserId, Guid chatId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}

