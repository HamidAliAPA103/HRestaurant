using HRestaurant.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HRestaurant.WebApi.Swagger;

public sealed class ManagementApiExamplesOperationFilter : IOperationFilter
{
    private static readonly Type[] Controllers =
    [
        typeof(UserController), typeof(ShiftController),
        typeof(MenuCategoryController), typeof(MenuController),
        typeof(IngredientController)
    ];

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var controller = context.MethodInfo.DeclaringType;
        if (controller is null || !Controllers.Contains(controller)) return;

        AddRequestExample(operation, controller, context.MethodInfo.Name);
        AddSuccessExample(operation, controller, context.MethodInfo.Name);
    }

    private static void AddRequestExample(
        OpenApiOperation operation, Type controller, string method)
    {
        if (method is not ("Create" or "AssignEmployee" or "AddToMenuItem")) return;

        IOpenApiAny? example = controller == typeof(UserController)
            ? new OpenApiObject
            {
                ["restaurantId"] = Id("11111111-1111-1111-1111-111111111111"),
                ["branchId"] = Id("22222222-2222-2222-2222-222222222222"),
                ["email"] = new OpenApiString("chef@example.com"),
                ["name"] = new OpenApiString("Aylin Məmmədova"),
                ["phone"] = new OpenApiString("+994501234567"),
                ["role"] = new OpenApiString("Chef"),
                ["salary"] = new OpenApiDouble(1200),
                ["hireDate"] = new OpenApiString("2026-07-31"),
                ["emergencyContact"] = new OpenApiString("+994551234567"),
                ["password"] = new OpenApiString("Strong!Pass1")
            }
            : controller == typeof(ShiftController) && method == "AssignEmployee"
                ? new OpenApiObject
                {
                    ["employeeId"] = Id("33333333-3333-3333-3333-333333333333"),
                    ["shiftId"] = Id("44444444-4444-4444-4444-444444444444"),
                    ["workDate"] = new OpenApiString("2026-08-03"),
                    ["startTime"] = new OpenApiString("09:00"),
                    ["endTime"] = new OpenApiString("17:00"),
                    ["notes"] = new OpenApiString("Main hall"),
                    ["status"] = new OpenApiString("Scheduled")
                }
            : controller == typeof(ShiftController)
                ? new OpenApiObject
                {
                    ["restaurantId"] = Id("11111111-1111-1111-1111-111111111111"),
                    ["branchId"] = Id("22222222-2222-2222-2222-222222222222"),
                    ["name"] = new OpenApiString("Morning"),
                    ["startTime"] = new OpenApiString("09:00"),
                    ["endTime"] = new OpenApiString("17:00")
                }
            : controller == typeof(MenuCategoryController)
                ? new OpenApiObject
                {
                    ["restaurantId"] = Id("11111111-1111-1111-1111-111111111111"),
                    ["name"] = new OpenApiString("Main courses"),
                    ["description"] = new OpenApiString("Chef specials"),
                    ["displayOrder"] = new OpenApiInteger(1)
                }
            : controller == typeof(IngredientController) && method == "AddToMenuItem"
                ? new OpenApiObject
                {
                    ["ingredientId"] = Id("55555555-5555-5555-5555-555555555555"),
                    ["requiredQuantity"] = new OpenApiDouble(150)
                }
            : controller == typeof(IngredientController)
                ? new OpenApiObject
                {
                    ["restaurantId"] = Id("11111111-1111-1111-1111-111111111111"),
                    ["name"] = new OpenApiString("Tomato"),
                    ["unit"] = new OpenApiString("Gram")
                }
            : null;

        if (example is null || operation.RequestBody is null) return;
        foreach (var mediaType in operation.RequestBody.Content.Values)
            mediaType.Example = example;
    }

    private static void AddSuccessExample(
        OpenApiOperation operation, Type controller, string method)
    {
        if (operation.Responses.ContainsKey("201"))
        {
            SetResponse(operation, "201", Response(
                "Resource created successfully.",
                Id("66666666-6666-6666-6666-666666666666"), 201));
            return;
        }

        if (!operation.Responses.ContainsKey("200")) return;
        var data = controller == typeof(UserController)
            ? EmployeeExample()
            : controller == typeof(ShiftController)
                ? ShiftExample(method.Contains("Assignment", StringComparison.OrdinalIgnoreCase)
                    || method is "GetDaily" or "GetWeekly" or "GetByEmployee" or "GetByBranch")
            : controller == typeof(MenuCategoryController)
                ? CategoryExample()
            : controller == typeof(IngredientController)
                ? IngredientExample()
            : MenuExample();
        SetResponse(operation, "200", Response("Request completed successfully.", data, 200));
    }

    private static OpenApiObject EmployeeExample() => new()
    {
        ["id"] = Id("33333333-3333-3333-3333-333333333333"),
        ["name"] = new OpenApiString("Aylin Məmmədova"),
        ["email"] = new OpenApiString("chef@example.com"),
        ["role"] = new OpenApiString("Chef"),
        ["isActive"] = new OpenApiBoolean(true)
    };

    private static OpenApiObject ShiftExample(bool assignment) => assignment
        ? new OpenApiObject
        {
            ["employeeName"] = new OpenApiString("Aylin Məmmədova"),
            ["shiftName"] = new OpenApiString("Morning"),
            ["workDate"] = new OpenApiString("2026-08-03"),
            ["startTime"] = new OpenApiString("09:00"),
            ["endTime"] = new OpenApiString("17:00")
        }
        : new OpenApiObject
        {
            ["name"] = new OpenApiString("Morning"),
            ["startTime"] = new OpenApiString("09:00"),
            ["endTime"] = new OpenApiString("17:00"),
            ["isActive"] = new OpenApiBoolean(true)
        };

    private static OpenApiObject CategoryExample() => new()
    {
        ["name"] = new OpenApiString("Main courses"),
        ["displayOrder"] = new OpenApiInteger(1),
        ["isActive"] = new OpenApiBoolean(true)
    };

    private static OpenApiObject IngredientExample() => new()
    {
        ["name"] = new OpenApiString("Tomato"),
        ["unit"] = new OpenApiString("Gram"),
        ["isActive"] = new OpenApiBoolean(true)
    };

    private static OpenApiObject MenuExample() => new()
    {
        ["name"] = new OpenApiString("Margherita Pizza"),
        ["price"] = new OpenApiDouble(15),
        ["discountPercentage"] = new OpenApiDouble(10),
        ["finalPrice"] = new OpenApiDouble(13.5),
        ["isAvailable"] = new OpenApiBoolean(true),
        ["ingredients"] = new OpenApiArray { IngredientExample() }
    };

    private static OpenApiObject Response(string message, IOpenApiAny data, int status) => new()
    {
        ["success"] = new OpenApiBoolean(true),
        ["message"] = new OpenApiString(message),
        ["data"] = data,
        ["errors"] = new OpenApiArray(),
        ["statusCode"] = new OpenApiInteger(status),
        ["traceId"] = new OpenApiString("00-management-example-trace-id-01")
    };

    private static OpenApiString Id(string value) => new(value);

    private static void SetResponse(
        OpenApiOperation operation, string status, IOpenApiAny example)
    {
        if (!operation.Responses.TryGetValue(status, out var response)) return;
        foreach (var mediaType in response.Content.Values)
            mediaType.Example = example;
    }
}
