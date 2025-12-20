using Application.DTOs.Image;
using FluentValidation;

namespace Application.Validators;

public class SaveProfilePictureRequestDtoValidator : AbstractValidator<SaveProfilePictureRequestDto>
{
    public SaveProfilePictureRequestDtoValidator()
    {
        RuleFor(x => x.PublicId)
            .NotEmpty().WithMessage("PublicId is required")
            .MaximumLength(500).WithMessage("PublicId must not exceed 500 characters");

        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("Url is required")
            .Must(BeAValidUrl).WithMessage("Url must be a valid URL")
            .MaximumLength(2000).WithMessage("Url must not exceed 2000 characters");
    }

    private static bool BeAValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}

