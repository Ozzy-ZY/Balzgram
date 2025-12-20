using Application.DTOs.Image;

namespace Application.Interfaces;

public interface IImageService
{
    ProfilePictureUploadSignatureResponseDto GetProfilePictureUploadSignature(string userId);
    Task<ImageDto> SaveProfilePictureAsync(string userId, SaveProfilePictureRequestDto request, CancellationToken cancellationToken = default);

    Task<ImageDto> GetProfilePictureAsync(string userId, CancellationToken cancellationToken = default);
    Task DeleteProfilePictureAsync(string userId, CancellationToken cancellationToken = default);
}

