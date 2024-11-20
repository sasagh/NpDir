using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace NpDirectory.Application.Validators;

public class ImageValidator : AbstractValidator<IFormFile>
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".bmp"];
    private static readonly string AllowedExtensionString = string.Join(", ", AllowedExtensions);

    public ImageValidator(IStringLocalizer localizer)
    {
        RuleFor(x => x)
            .Must(IsImage).WithMessage(localizer["Error.Image.Invalid", AllowedExtensionString])
            .When(x => x != null);
    }

    private static bool IsImage(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return AllowedExtensions.Contains(extension);
    }
}