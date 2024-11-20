using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using NpDirectory.Application.Validators;

namespace NpDirectory.Application.Requests;

public class UpdateNaturalPersonImageRequest
{
    public IFormFile Image { get; set; }
}

public class ValidateUpdateNaturalPersonImageRequest : AbstractValidator<UpdateNaturalPersonImageRequest>
{
    public ValidateUpdateNaturalPersonImageRequest(IStringLocalizer localizer)
    {
        RuleFor(x => x.Image)
            .NotNull()
            .WithMessage(localizer["Error.Image.Required"])
            .SetValidator(new ImageValidator(localizer));
    }
}