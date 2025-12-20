namespace Application.DTOs.Image;

public record ProfilePictureUploadSignatureResponseDto(
    string Signature,
    long Timestamp,
    string ApiKey,
    string CloudName,
    string Folder);

public record SaveProfilePictureRequestDto(
    string PublicId,
    string Url);
public record ImageDto(
    Guid Id,
    string PublicId,
    string Url,
    DateTime CreatedAtUtc);


