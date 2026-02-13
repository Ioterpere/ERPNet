using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ERPNet.Api.Filters;

public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
                continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            var validator = context.HttpContext.RequestServices.GetService(validatorType) as IValidator;

            if (validator is null)
                continue;

            var validationContext = new ValidationContext<object>(argument);
            var result = await validator.ValidateAsync(validationContext);

            if (!result.IsValid)
            {
                var problemDetails = new ValidationProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Error de validacion."
                };

                foreach (var error in result.Errors)
                {
                    var field = string.IsNullOrEmpty(error.PropertyName) ? "General" : error.PropertyName;

                    if (!problemDetails.Errors.TryGetValue(field, out var existing))
                        problemDetails.Errors[field] = [error.ErrorMessage];
                    else
                        problemDetails.Errors[field] = [..existing, error.ErrorMessage];
                }

                context.Result = new ObjectResult(problemDetails)
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
                return;
            }
        }

        await next();
    }
}
