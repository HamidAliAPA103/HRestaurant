using HRestaurant.DTOS.Public;
using HRestaurant.DTOS.Responses;
using HRestaurant.Services.Interfaces;
using HRestaurant.WebApi.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;

namespace HRestaurant.Controllers;

[ApiController]
[AllowAnonymous]
[EnableRateLimiting(RateLimitPolicies.PublicGet)]
[Produces("application/json")]
[Route("api/public")]
public sealed class PublicRestaurantController : ApiControllerBase
{
    private readonly IPublicRestaurantService _restaurantService;
    private readonly ITableAvailabilityService _availabilityService;

    public PublicRestaurantController(
        IPublicRestaurantService restaurantService,
        ITableAvailabilityService availabilityService)
    {
        ArgumentNullException.ThrowIfNull(restaurantService);
        ArgumentNullException.ThrowIfNull(availabilityService);

        _restaurantService = restaurantService;
        _availabilityService = availabilityService;
    }

    [HttpGet("restaurants")]
    [SwaggerOperation(
        Summary = "List public restaurants",
        Description = "Returns active restaurants available on the public website.",
        Tags = ["Public Restaurant"])]
    [ProducesResponseType(
        typeof(ApiResponse<IReadOnlyCollection<PublicRestaurantDto>>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetRestaurants(CancellationToken cancellationToken)
    {
        return FromResponse(await _restaurantService.GetAllAsync(cancellationToken));
    }

    [HttpGet("restaurants/{slug}")]
    [SwaggerOperation(
        Summary = "Get a public restaurant",
        Description =
            "Returns only public restaurant content, current open state, "
            + "working hours and active branches. Authentication is not "
            + "required.",
        Tags = ["Public Restaurant"])]
    [ProducesResponseType(
        typeof(ApiResponse<PublicRestaurantDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRestaurant(
        string slug,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _restaurantService.GetBySlugAsync(
                slug,
                cancellationToken));
    }

    [HttpGet("restaurants/{restaurantSlug}/branches")]
    [SwaggerOperation(
        Summary = "Get active public branches",
        Description =
            "Returns active branches and branch-specific working hours.",
        Tags = ["Public Restaurant"])]
    [ProducesResponseType(
        typeof(ApiResponse<
            IReadOnlyCollection<PublicBranchDto>>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBranches(
        string restaurantSlug,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _restaurantService.GetBranchesAsync(
                restaurantSlug,
                cancellationToken));
    }

    [HttpGet("restaurants/{restaurantSlug}/menu")]
    [SwaggerOperation(
        Summary = "Get the public restaurant menu",
        Description = "Returns active menu categories and their non-deleted items without requiring authentication.",
        Tags = ["Public Restaurant"])]
    [ProducesResponseType(
        typeof(ApiResponse<IReadOnlyCollection<PublicMenuCategoryDto>>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetMenu(
        string restaurantSlug,
        CancellationToken cancellationToken)
    {
        return FromResponse(await _restaurantService.GetMenuAsync(
            restaurantSlug,
            cancellationToken));
    }

    [HttpGet("branches/{branchId:guid}/available-tables")]
    [HttpGet("branches/{branchId:guid}/tables")]
    [SwaggerOperation(
        Summary = "Get table availability",
        Description =
            "Returns active, non-deleted tables for the selected branch. "
            + "Example query: ?reservationDate=2026-08-10"
            + "&startTime=19:00&guestCount=4&durationMinutes=120.",
        Tags = ["Public Restaurant"])]
    [ProducesResponseType(
        typeof(ApiResponse<
            IReadOnlyCollection<PublicRestaurantTableDto>>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTables(
        Guid branchId,
        [FromQuery] TableAvailabilityRequestDto request,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _availabilityService.GetTablesAsync(
                branchId,
                request,
                cancellationToken));
    }
}
