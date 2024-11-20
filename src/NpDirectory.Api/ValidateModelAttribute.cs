using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NpDirectory.Api;

public class ValidateModelAttribute : ActionFilterAttribute
{
    private readonly IServiceProvider _serviceProvider;

    public ValidateModelAttribute(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var actionArguments = context.ActionArguments;

        foreach (var arg in actionArguments)
        {
            if (arg.Value == null)
                continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(arg.Value.GetType());

            if (_serviceProvider.GetService(validatorType) is IValidator validator)
            {
                var validationContext = new ValidationContext<object>(arg.Value);

                var validationResult = await validator.ValidateAsync(validationContext);

                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }).ToList();

                    context.Result = new BadRequestObjectResult(errors);
                    return;
                }
            }
        }

        await base.OnActionExecutionAsync(context, next);
    }
}