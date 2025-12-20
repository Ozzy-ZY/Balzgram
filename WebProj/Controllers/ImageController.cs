using System.Security.Claims;
using Application.DTOs;
using Application.DTOs.Image;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebProj.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ImageController(IImageService imageService) : ControllerBase
{
    /// <summary>
    /// Get a Cloudinary upload signature for profile picture upload
    /// </summary>
    [HttpGet("profile-picture/signature")]
    [ProducesResponseType(typeof(ProfilePictureUploadSignatureResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto),StatusCodes.Status401Unauthorized)]
    public IActionResult GetProfilePictureUploadSignature()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var signature = imageService.GetProfilePictureUploadSignature(userId);
        return Ok(signature);
    }

    /// <summary>
    /// Save the profile picture after successful Cloudinary upload
    /// </summary>
    [HttpPost("profile-picture")]
    [ProducesResponseType(typeof(ImageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto),StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SaveProfilePicture(
        [FromBody] SaveProfilePictureRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }
        var result = await imageService.SaveProfilePictureAsync(userId, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get the current user's profile picture
    /// </summary>
    [HttpGet("profile-picture")]
    [ProducesResponseType(typeof(ImageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto),StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponseDto),StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProfilePicture(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await imageService.GetProfilePictureAsync(userId,true, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific user's profile picture
    /// </summary>
    [HttpGet("profile-picture/{userName}")]
    [ProducesResponseType(typeof(ImageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto),StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponseDto),StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserProfilePicture(
        [FromRoute] string userName,
        CancellationToken cancellationToken)
    {
        var result = await imageService.GetProfilePictureAsync(userName,false, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Delete the current user's profile picture
    /// </summary>
    [HttpDelete("profile-picture")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponseDto),StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteProfilePicture(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        await imageService.DeleteProfilePictureAsync(userId, cancellationToken);
        return NoContent();
    }
}

