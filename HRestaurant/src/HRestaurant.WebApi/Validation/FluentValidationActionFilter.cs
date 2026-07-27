using System.Collections;
using FluentValidation;
using FluentValidation.Results;
using HRestaurant.DTOS.Common;
using HRestaurant.DTOS.Menu;
using HRestaurant.WebApi.Models.Menu;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HRestaurant.WebApi.Validation;

public sealed class FluentValidationActionFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public FluentValidationActionFilter(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        _serviceProvider = serviceProvider;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var failures = new List<ValidationFailure>();
        var cancellationToken = context.HttpContext.RequestAborted;

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var validationTarget = CreateValidationTarget(argument);

            foreach (var validator in GetValidators(
                         validationTarget.GetType()))
            {
                var validationContext =
                    new ValidationContext<object>(validationTarget);

                var validationResult = await validator.ValidateAsync(
                    validationContext,
                    cancellationToken);

                failures.AddRange(validationResult.Errors);
            }
        }

        if (failures.Count > 0)
        {
            var response =
                ValidationErrorResponseFactory.FromFailures(failures);

            context.Result = new ObjectResult(response)
            {
                StatusCode = response.StatusCode
            };

            return;
        }

        await next();
    }

    private IEnumerable<IValidator> GetValidators(Type modelType)
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(modelType);
        var collectionType = typeof(IEnumerable<>).MakeGenericType(
            validatorType);

        if (_serviceProvider.GetService(collectionType) is not
            IEnumerable validators)
        {
            yield break;
        }

        foreach (var validator in validators)
        {
            if (validator is IValidator fluentValidator)
            {
                yield return fluentValidator;
            }
        }
    }

    private static object CreateValidationTarget(object argument)
    {
        return argument switch
        {
            MenuCreateRequest request => CreateMenuCreateDTO(request),
            MenuUpdateRequest request => CreateMenuUpdateDTO(request),
            _ => argument
        };
    }

    private static MenuCreateDTO CreateMenuCreateDTO(
        MenuCreateRequest request)
    {
        return new MenuCreateDTO
        {
            Image = request.Image is null
                ? null!
                : CreateFileUploadDTO(request.Image),
            Price = request.Price,
            Desc = request.Desc,
            CategoryId = request.CategoryId,
            Nutrition = request.Nutrition
        };
    }

    private static MenuUpdateDTO CreateMenuUpdateDTO(
        MenuUpdateRequest request)
    {
        return new MenuUpdateDTO
        {
            Image = request.Image is null
                ? null
                : CreateFileUploadDTO(request.Image),
            ImageURL = request.ImageURL,
            Price = request.Price,
            Desc = request.Desc,
            Nutrition = request.Nutrition
        };
    }

    private static FileUploadDTO CreateFileUploadDTO(IFormFile file)
    {
        return new FileUploadDTO
        {
            Content = Stream.Null,
            FileName = file.FileName,
            ContentType = file.ContentType,
            Length = file.Length
        };
    }
}
