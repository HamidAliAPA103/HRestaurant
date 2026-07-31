using HRestaurant.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HRestaurant.WebApi.Swagger;

public sealed class PublicApiExamplesOperationFilter
    : IOperationFilter
{
    public void Apply(
        OpenApiOperation operation,
        OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType
            == typeof(PublicTableLayoutController))
        {
            operation.Security.Clear();
            SetResponseExample(
                operation,
                "200",
                SuccessResponse(
                    "Public table layout retrieved successfully.",
                    new OpenApiArray { PublicTableLayoutExample() }));
            AddCommonErrorExamples(operation);
            return;
        }

        if (context.MethodInfo.DeclaringType
            == typeof(PublicRestaurantController))
        {
            operation.Security.Clear();
            AddRestaurantExamples(
                operation,
                context.MethodInfo.Name);
            AddCommonErrorExamples(operation);
            return;
        }

        if (context.MethodInfo.DeclaringType
            != typeof(PublicReservationsController))
        {
            return;
        }

        operation.Security.Clear();

        switch (context.MethodInfo.Name)
        {
            case nameof(PublicReservationsController.Create):
                SetRequestExample(
                    operation,
                    new OpenApiObject
                    {
                        ["branchId"] = new OpenApiString(
                            "a3a0abde-5c00-42d8-877a-08302d670ae3"),
                        ["tableId"] = new OpenApiString(
                            "0ad04f21-68be-4f06-bbd8-889704f2fcf3"),
                        ["reservationDate"] =
                            new OpenApiString("2026-08-10"),
                        ["startTime"] = new OpenApiString("19:00"),
                        ["durationMinutes"] = new OpenApiInteger(120),
                        ["guestCount"] = new OpenApiInteger(4),
                        ["fullName"] =
                            new OpenApiString("Aydan Sharifova"),
                        ["phone"] =
                            new OpenApiString("+994501234567"),
                        ["email"] =
                            new OpenApiString("example@email.com"),
                        ["specialNotes"] =
                            new OpenApiString("Window-side table"),
                        ["termsAccepted"] = new OpenApiBoolean(true)
                    });
                SetResponseExample(
                    operation,
                    "201",
                    new OpenApiObject
                    {
                        ["success"] = new OpenApiBoolean(true),
                        ["message"] = new OpenApiString(
                            "Reservation created successfully."),
                        ["data"] = new OpenApiObject
                        {
                            ["reservationId"] = new OpenApiString(
                                "f7917478-96d0-49ee-83ed-e8c557642286"),
                            ["confirmationCode"] =
                                new OpenApiString("RSV-8F3K2M"),
                            ["trackingToken"] = new OpenApiString(
                                new string('a', 64)),
                            ["status"] = new OpenApiString("Pending"),
                            ["restaurantName"] =
                                new OpenApiString("Restaurant Name"),
                            ["branchName"] =
                                new OpenApiString("Baku Branch"),
                            ["tableNumber"] =
                                new OpenApiString("T-12"),
                            ["reservationDate"] =
                                new OpenApiString("2026-08-10"),
                            ["startTime"] =
                                new OpenApiString("19:00"),
                            ["endTime"] =
                                new OpenApiString("21:00"),
                            ["emailDeliveryQueued"] =
                                new OpenApiBoolean(true)
                        },
                        ["errors"] = new OpenApiArray(),
                        ["statusCode"] = new OpenApiInteger(201),
                        ["traceId"] = new OpenApiString(
                            "00-example-trace-id-01")
                    });
                break;

            case nameof(PublicReservationsController.Lookup):
                SetRequestExample(
                    operation,
                    new OpenApiObject
                    {
                        ["confirmationCode"] =
                            new OpenApiString("RSV-8F3K2M"),
                        ["phone"] =
                            new OpenApiString("+994501234567")
                    });
                SetResponseExample(
                    operation,
                    "200",
                    SuccessResponse(
                        "Reservation retrieved successfully.",
                        ReservationDetailsExample()));
                break;

            case nameof(PublicReservationsController.Track):
                SetResponseExample(
                    operation,
                    "200",
                    SuccessResponse(
                        "Reservation retrieved successfully.",
                        ReservationDetailsExample()));
                break;

            case nameof(PublicReservationsController.Cancel):
                SetRequestExample(
                    operation,
                    new OpenApiObject
                    {
                        ["phone"] =
                            new OpenApiString("+994501234567"),
                        ["reason"] =
                            new OpenApiString("Plans changed")
                    });
                SetResponseExample(
                    operation,
                    "200",
                    SuccessResponse(
                        "Reservation cancelled successfully.",
                        new OpenApiNull()));
                break;
        }

        AddCommonErrorExamples(operation);
    }

    private static void AddRestaurantExamples(
        OpenApiOperation operation,
        string methodName)
    {
        switch (methodName)
        {
            case nameof(PublicRestaurantController.GetRestaurant):
                SetResponseExample(
                    operation,
                    "200",
                    SuccessResponse(
                        "Restaurant retrieved successfully.",
                        RestaurantExample()));
                break;

            case nameof(PublicRestaurantController.GetBranches):
                SetResponseExample(
                    operation,
                    "200",
                    SuccessResponse(
                        "Branches retrieved successfully.",
                        new OpenApiArray
                        {
                            BranchExample()
                        }));
                break;

            case nameof(PublicRestaurantController.GetTables):
                AddAvailabilityExamples(operation);
                SetResponseExample(
                    operation,
                    "200",
                    SuccessResponse(
                        "Table availability retrieved successfully.",
                        new OpenApiArray
                        {
                            TableExample()
                        }));
                break;
        }
    }

    private static void AddAvailabilityExamples(
        OpenApiOperation operation)
    {
        foreach (var parameter in operation.Parameters)
        {
            parameter.Example = parameter.Name switch
            {
                "reservationDate" =>
                    new OpenApiString("2026-08-10"),
                "startTime" => new OpenApiString("19:00"),
                "guestCount" => new OpenApiInteger(4),
                "durationMinutes" => new OpenApiInteger(120),
                _ => parameter.Example
            };
        }
    }

    private static void SetRequestExample(
        OpenApiOperation operation,
        IOpenApiAny example)
    {
        if (operation.RequestBody?.Content.TryGetValue(
                "application/json",
                out var mediaType)
            == true)
        {
            mediaType.Example = example;
        }
    }

    private static void SetResponseExample(
        OpenApiOperation operation,
        string statusCode,
        IOpenApiAny example)
    {
        if (operation.Responses.TryGetValue(
                statusCode,
                out var response)
            && response.Content.TryGetValue(
                "application/json",
                out var mediaType))
        {
            mediaType.Example = example;
        }
    }

    private static void AddCommonErrorExamples(
        OpenApiOperation operation)
    {
        var examples = new Dictionary<string, (string Code, string Message)>
        {
            ["400"] = (
                "validation_error",
                "One or more validation errors occurred."),
            ["404"] = (
                "not_found",
                "The requested resource was not found."),
            ["409"] = (
                "conflict",
                "The selected table is no longer available."),
            ["429"] = (
                "rate_limit_exceeded",
                "Too many requests. Please try again later."),
            ["500"] = (
                "internal_server_error",
                "An unexpected error occurred.")
        };

        foreach (var (statusCode, definition) in examples)
        {
            SetResponseExample(
                operation,
                statusCode,
                FailureResponse(
                    int.Parse(statusCode),
                    definition.Code,
                    definition.Message));
        }
    }

    private static OpenApiObject SuccessResponse(
        string message,
        IOpenApiAny data)
    {
        return new OpenApiObject
        {
            ["success"] = new OpenApiBoolean(true),
            ["message"] = new OpenApiString(message),
            ["data"] = data,
            ["errors"] = new OpenApiArray(),
            ["statusCode"] = new OpenApiInteger(200),
            ["traceId"] = new OpenApiString(
                "00-example-trace-id-01")
        };
    }

    private static OpenApiObject FailureResponse(
        int statusCode,
        string code,
        string message)
    {
        return new OpenApiObject
        {
            ["success"] = new OpenApiBoolean(false),
            ["message"] = new OpenApiString(message),
            ["data"] = new OpenApiNull(),
            ["errors"] = new OpenApiArray
            {
                new OpenApiObject
                {
                    ["code"] = new OpenApiString(code),
                    ["message"] = new OpenApiString(message)
                }
            },
            ["statusCode"] = new OpenApiInteger(statusCode),
            ["traceId"] = new OpenApiString(
                "00-example-trace-id-01")
        };
    }

    private static OpenApiObject RestaurantExample()
    {
        return new OpenApiObject
        {
            ["id"] = new OpenApiString(
                "0157e7ed-e609-455f-a99f-08dee0c7480e"),
            ["slug"] = new OpenApiString("restaurant-name"),
            ["name"] = new OpenApiString("Restaurant Name"),
            ["description"] = new OpenApiString(
                "Seasonal cuisine in a relaxed setting."),
            ["phone"] = new OpenApiString("+994501234567"),
            ["email"] = new OpenApiString("hello@example.com"),
            ["address"] = new OpenApiString("Baku, Azerbaijan"),
            ["isOpenNow"] = new OpenApiBoolean(true),
            ["workingHours"] = new OpenApiArray
            {
                WorkingHourExample()
            },
            ["branches"] = new OpenApiArray
            {
                BranchExample()
            }
        };
    }

    private static OpenApiObject BranchExample()
    {
        return new OpenApiObject
        {
            ["id"] = new OpenApiString(
                "a3a0abde-5c00-42d8-877a-08302d670ae3"),
            ["name"] = new OpenApiString("Baku Branch"),
            ["slug"] = new OpenApiString("baku-branch"),
            ["address"] = new OpenApiString("Baku, Azerbaijan"),
            ["phone"] = new OpenApiString("+994501234567"),
            ["timeZoneId"] = new OpenApiString("Asia/Baku"),
            ["isOpenNow"] = new OpenApiBoolean(true),
            ["workingHours"] = new OpenApiArray
            {
                WorkingHourExample()
            }
        };
    }

    private static OpenApiObject WorkingHourExample()
    {
        return new OpenApiObject
        {
            ["dayOfWeek"] = new OpenApiString("Monday"),
            ["dayName"] = new OpenApiString("Monday"),
            ["opensAt"] = new OpenApiString("10:00"),
            ["closesAt"] = new OpenApiString("23:00"),
            ["isClosed"] = new OpenApiBoolean(false)
        };
    }

    private static OpenApiObject TableExample()
    {
        return new OpenApiObject
        {
            ["id"] = new OpenApiString(
                "0ad04f21-68be-4f06-bbd8-889704f2fcf3"),
            ["tableNumber"] = new OpenApiString("T-12"),
            ["capacity"] = new OpenApiInteger(4),
            ["shape"] = new OpenApiString("Rectangle"),
            ["positionX"] = new OpenApiDouble(2),
            ["positionY"] = new OpenApiDouble(0),
            ["positionZ"] = new OpenApiDouble(3),
            ["rotationX"] = new OpenApiDouble(0),
            ["rotationY"] = new OpenApiDouble(0.5),
            ["rotationZ"] = new OpenApiDouble(0),
            ["width"] = new OpenApiDouble(1.8),
            ["length"] = new OpenApiDouble(1.2),
            ["height"] = new OpenApiDouble(0.75),
            ["status"] = new OpenApiString("Available"),
            ["isAvailable"] = new OpenApiBoolean(true),
            ["unavailableReason"] = new OpenApiNull()
        };
    }

    private static OpenApiObject PublicTableLayoutExample()
    {
        return new OpenApiObject
        {
            ["id"] = new OpenApiString(
                "0ad04f21-68be-4f06-bbd8-889704f2fcf3"),
            ["tableNumber"] = new OpenApiString("T-12"),
            ["capacity"] = new OpenApiInteger(4),
            ["shape"] = new OpenApiString("Rectangle"),
            ["position"] = new OpenApiObject
            {
                ["x"] = new OpenApiDouble(2),
                ["y"] = new OpenApiDouble(0),
                ["z"] = new OpenApiDouble(3)
            },
            ["rotation"] = new OpenApiObject
            {
                ["x"] = new OpenApiDouble(0),
                ["y"] = new OpenApiDouble(0.5),
                ["z"] = new OpenApiDouble(0)
            },
            ["dimensions"] = new OpenApiObject
            {
                ["width"] = new OpenApiDouble(1.8),
                ["length"] = new OpenApiDouble(1.2),
                ["height"] = new OpenApiDouble(0.75)
            },
            ["publicStatus"] = new OpenApiString("Available")
        };
    }

    private static OpenApiObject ReservationDetailsExample()
    {
        return new OpenApiObject
        {
            ["confirmationCode"] =
                new OpenApiString("RSV-8F3K2M"),
            ["status"] = new OpenApiString("Pending"),
            ["restaurantName"] =
                new OpenApiString("Restaurant Name"),
            ["branchName"] = new OpenApiString("Baku Branch"),
            ["branchAddress"] =
                new OpenApiString("Baku, Azerbaijan"),
            ["reservationDate"] =
                new OpenApiString("2026-08-10"),
            ["startTime"] = new OpenApiString("19:00"),
            ["endTime"] = new OpenApiString("21:00"),
            ["guestCount"] = new OpenApiInteger(4),
            ["tableNumber"] = new OpenApiString("T-12"),
            ["fullName"] = new OpenApiString("Aydan Sharifova"),
            ["maskedPhone"] = new OpenApiString("+99450*****67"),
            ["maskedEmail"] =
                new OpenApiString("a***n@example.com"),
            ["specialNotes"] =
                new OpenApiString("Window-side table"),
            ["canCancel"] = new OpenApiBoolean(true),
            ["cancelledAt"] = new OpenApiNull()
        };
    }
}
