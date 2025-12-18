using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class ErrorResponseDto
{
    [Required]
    public ErrorInfoDto Error { get; set; } = new ErrorInfoDto();
}

public class ErrorInfoDto
{
    [Required]
    public string Message { get; set; } = string.Empty;
    [Required]
    public string Type { get; set; } = string.Empty;
    public string? Details { get; set; }
}

