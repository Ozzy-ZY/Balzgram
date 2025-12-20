using Application.DTOs;
using Application.DTOs.Image;
using Application.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class CloudinaryService(IOptions<CloudinarySettings> cloudinaryConfig) :ICloudinaryService
{
    private readonly Cloudinary _cloudinary = new Cloudinary(new Account(
        cloudinaryConfig.Value.CloudName,
        cloudinaryConfig.Value.ApiKey,
        cloudinaryConfig.Value.ApiSecret));
    private readonly CloudinarySettings _cloudinarySettings = cloudinaryConfig.Value;
    
    public CloudinarySignatureDto GetUploadSignature(CloudinaryUploadRequestDto request)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var tagsString = string.Join(",", request.Tags);
        var publicId = Guid.NewGuid().ToString();
        
        var parameters = new SortedDictionary<string, object>
        {
            { "folder", request.Folder },
            { "public_id", publicId },
            { "tags", tagsString },
            { "timestamp", timestamp }
        };
        var signature = _cloudinary.Api.SignParameters(parameters);
        return new CloudinarySignatureDto(
            Signature: signature,
            Timestamp: timestamp,
            ApiKey: _cloudinarySettings.ApiKey,
            CloudName: _cloudinarySettings.CloudName,
            Folder: request.Folder,
            Tags: request.Tags,
            PublicId: publicId);
    }

    public async Task<bool> DeleteFileAsync(string publicId)
    {
        var result = await _cloudinary.DestroyAsync(
            new DeletionParams(publicId));
        return result.Result == "ok";
    }
}