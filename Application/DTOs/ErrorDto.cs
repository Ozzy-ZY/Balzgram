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
public class ValidationErrorResponseDto
{
    [Required]
    public string Type { get; set; }
    [Required]
    public string Title { get; set; }
    [Required]
    public int Status { get; set; }
    
    [Required]
    public Dictionary<string, string[]> Errors { get; set; } = new();
    public string? TraceId { get; set; }
}

