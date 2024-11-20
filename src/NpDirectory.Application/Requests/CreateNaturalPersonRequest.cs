using FluentValidation;
using FluentValidation.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using NpDirectory.Application.Common;
using NpDirectory.Application.Repositories;
using NpDirectory.Application.Validators;
using NpDirectory.Domain.Enum;

namespace NpDirectory.Application.Requests;

public class CreateNaturalPersonRequest
{
    public string FirstName { get; set; }
    
    public string LastName { get; set; }
    
    public Sex Sex { get; set; }
    
    public string PersonalNumber { get; set; }
    
    public DateTime BirthDate { get; set; }
    
    public int CityId { get; set; }
    
    public List<PhoneNumberModel> PhoneNumbers { get; set; }
}

public class CreateNaturalPersonRequestValidator : AbstractValidator<CreateNaturalPersonRequest>
{
    public CreateNaturalPersonRequestValidator(
        ICityRepository cityRepository,
        IStringLocalizer localizer)
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage(localizer["Validation.Name.Required"])
            .MinimumLength(2)
            .WithMessage(localizer["Validation.Name.TooShort"])
            .MaximumLength(50)
            .WithMessage(localizer["Validation.Name.TooLong"])
            .SetValidator(new Validators.NameValidator(localizer))
            .WithMessage(localizer["Validation.Name.Invalid"]);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage(localizer["Validation.Name.Required"])
            .MinimumLength(2)
            .WithMessage(localizer["Validation.Name.TooShort"])
            .MaximumLength(50)
            .WithMessage(localizer["Validation.Name.TooLong"])
            .SetValidator(new Validators.NameValidator(localizer))
            .WithMessage(localizer["Validation.Name.Invalid"]);

        RuleFor(x => x.Sex)
            .IsInEnum()
            .WithMessage(localizer["Validation.Sex.Invalid"]);

        RuleFor(x => x.PersonalNumber)
            .NotEmpty()
            .WithMessage(localizer["Validation.PersonalNumber.Required"])
            .Matches(@"^\d{11}$")
            .WithMessage(localizer["Validation.PersonalNumber.Invalid"]);

        RuleFor(x => x.CityId)
            .SetAsyncValidator(new CityValidator(cityRepository, localizer));

        RuleFor(x => x.BirthDate)
            .NotEmpty()
            .WithMessage(localizer["Validation.BirthDate.Required"])
            .LessThan(DateTime.UtcNow.Date)
            .WithMessage(localizer["Validation.BirthDate.Future"])
            .LessThanOrEqualTo(DateTime.UtcNow.AddYears(-18).Date)
            .WithMessage(localizer["Validation.BirthDate.TooYoung"]);

        RuleFor(x => x.PhoneNumbers)
            .NotEmpty()
            .WithMessage(localizer["Validation.PhoneNumber.Required"]);
        
        RuleForEach(x => x.PhoneNumbers).ChildRules(phoneNumber =>
        {
            phoneNumber
                .RuleFor(p => p.Type)
                .IsInEnum()
                .WithMessage(localizer["Validation.PhoneNumber.Type.Invalid"]);
            
            phoneNumber
                .RuleFor(p => p.Number)
                .NotEmpty()
                .WithMessage(localizer["Validation.PhoneNumber.Required"])
                .MinimumLength(2)
                .WithMessage(localizer["Validation.PhoneNumber.TooShort"])
                .MaximumLength(50)
                .WithMessage(localizer["Validation.PhoneNumber.TooLong"]);
        });
    }
    
    class CityValidator : AsyncPropertyValidator<CreateNaturalPersonRequest, int>
    {
        private readonly ICityRepository _cityRepository;
        private readonly IStringLocalizer _localizer;

        public CityValidator(ICityRepository cityRepository, IStringLocalizer localizer)
        {
            _cityRepository = cityRepository;
            _localizer = localizer;
        }

        public override async Task<bool> IsValidAsync(ValidationContext<CreateNaturalPersonRequest> context, int cityId, CancellationToken cancellation)
        {
            var city = await _cityRepository.GetOneByIdAsync(cityId);
            return city != null;
        }

        public override string Name { get; } = "CityValidator";
    
        protected override string GetDefaultMessageTemplate(string errorCode)
            => _localizer["Validation.City.NotFound"];
    }
}
