using System.Text.RegularExpressions;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace NpDirectory.Application.Validators;

public partial class NameValidator : AbstractValidator<string>
{
    public NameValidator(IStringLocalizer localizer)
    {
        RuleFor(x => x).Must(IsValidName).WithMessage(localizer["Validation.Name.Invalid"]);
    }

    private bool IsValidName(string name)
    {
        var isGeorgian = GeorgianRegex().IsMatch(name);
        var isEnglish = EnglishRegex().IsMatch(name);
        
        return isGeorgian || isEnglish;
    }

    [GeneratedRegex(@"^[\u10D0-\u10F0]+$")]
    private static partial Regex GeorgianRegex();
    [GeneratedRegex(@"^[a-zA-Z]+$")]
    private static partial Regex EnglishRegex();
}