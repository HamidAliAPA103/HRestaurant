using HRestaurant.DTOS.Responses;
using Microsoft.AspNetCore.Mvc;

namespace HRestaurant.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult FromResponse<T>(ApiResponse<T> response)
    {
        ArgumentNullException.ThrowIfNull(response);

        if (!response.Success && string.IsNullOrWhiteSpace(response.TraceId))
        {
            response = response.WithTraceId(HttpContext.TraceIdentifier);
        }

        return response.StatusCode == StatusCodes.Status204NoContent
            ? NoContent()
            : StatusCode(response.StatusCode, response);
    }
}
