namespace Application.DTOs.Image;

public record CloudinaryUploadRequestDto(
    string Folder,
    List<string> Tags
);
