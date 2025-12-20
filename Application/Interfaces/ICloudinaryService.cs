using Application.DTOs;
using Application.DTOs.Image;

namespace Application.Interfaces;

public interface ICloudinaryService
{
    CloudinarySignatureDto GetUploadSignature(CloudinaryUploadRequestDto request);
    Task<bool> DeleteFileAsync(string publicId);
}