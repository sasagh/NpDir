using FluentValidation;
using Microsoft.Extensions.Localization;
using NpDirectory.Domain.Enum;

namespace NpDirectory.Application.Requests;

public class CreateRelationRequest
{
    public int FromId { get; set; }
    
    public int ToId { get; set; }
    
    public RelationType RelationType { get; set; }
}

public class ValidateCreateRelationRequest : AbstractValidator<CreateRelationRequest>
{
    public ValidateCreateRelationRequest(IStringLocalizer localizer)
    {
        RuleFor(x => x.FromId)
            .NotEmpty()
            .WithMessage(localizer["Error.Id.Empty"])
            .GreaterThan(0)
            .WithMessage(localizer["Error.Id.GreaterThanZero"]);
        
        RuleFor(x => x.ToId)
            .NotEmpty()
            .WithMessage(localizer["Error.Id.Empty"])
            .GreaterThan(0)
            .WithMessage(localizer["Error.Id.GreaterThanZero"]);

        RuleFor(x => x.RelationType)
            .IsInEnum();
        
        RuleFor(x => x)
            .Must(x => x.FromId != x.ToId)
            .WithMessage(localizer["Error.Relation.SamePerson"]);
    }
}