using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AssignmentHub.Api.Filters;

/// <summary>
/// Runs before every action: for each action argument that has a registered FluentValidation
/// IValidator&lt;T&gt;, validates it and short-circuits with a 400 ProblemDetails (field errors in
/// the "errors" extension) if invalid. Keeps controllers thin — they never call a validator
/// themselves, and Application services can assume any DTO they receive is already well-formed.
/// </summary>
public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var errors = new Dictionary<string, string[]>();

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var validationContext = new ValidationContext<object>(argument);
            var result = await validator.ValidateAsync(validationContext);

            if (!result.IsValid)
            {
                foreach (var group in result.Errors.GroupBy(e => e.PropertyName))
                {
                    errors[group.Key] = group.Select(e => e.ErrorMessage).ToArray();
                }
            }
        }

        if (errors.Count > 0)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed",
                Instance = context.HttpContext.Request.Path
            };
            problemDetails.Extensions["errors"] = errors;

            context.Result = new BadRequestObjectResult(problemDetails)
            {
                ContentTypes = { "application/problem+json" }
            };
            return;
        }

        await next();
    }
}
