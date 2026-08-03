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

    [HttpGet("restaurants/{restaurantSlug}/experience")]
    [SwaggerOperation(
        Summary = "Get public restaurant experience",
        Description = "Returns public restaurant and active branch information used by the virtual tour.",
        Tags = ["Public Restaurant Experience"])]
    [ProducesResponseType(
        typeof(ApiResponse<PublicRestaurantExperienceDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetExperience(
        string restaurantSlug,
        CancellationToken cancellationToken) =>
        FromResponse(await _restaurantService.GetExperienceAsync(
            restaurantSlug,
            cancellationToken));

    [HttpGet("restaurants/{restaurantSlug}/scene")]
    [SwaggerOperation(
        Summary = "Get public restaurant 3D scene",
        Description = "Returns a procedural scene calculated from real branch and table layout data.",
        Tags = ["Public Restaurant Experience"])]
    [ProducesResponseType(
        typeof(ApiResponse<PublicRestaurantSceneDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetScene(
        string restaurantSlug,
        CancellationToken cancellationToken) =>
        FromResponse(await _restaurantService.GetSceneAsync(
            restaurantSlug,
            cancellationToken));

    [HttpGet("menu-items/{menuItemId:guid}/3d")]
    [SwaggerOperation(
        Summary = "Get public 3D menu item data",
        Description = "Returns only public presentation and 3D model settings. Inventory, supplier and cost data are never exposed.",
        Tags = ["Public Restaurant"])]
    [ProducesResponseType(typeof(ApiResponse<PublicMenuItem3DDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetMenuItem3D(
        Guid menuItemId,
        CancellationToken cancellationToken) =>
        FromResponse(await _restaurantService.GetMenuItem3DAsync(
            menuItemId,
            cancellationToken));

    [HttpGet("menu-items/{menuItemId:guid}/ingredients-3d")]
    [SwaggerOperation(
        Summary = "Get public 3D ingredient data",
        Description = "Returns public ingredient, nutrition and exploded-view transforms without inventory prices or supplier information.",
        Tags = ["Public Restaurant"])]
    [ProducesResponseType(
        typeof(ApiResponse<IReadOnlyCollection<PublicIngredient3DDto>>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetMenuItemIngredients3D(
        Guid menuItemId,
        CancellationToken cancellationToken) =>
        FromResponse(await _restaurantService.GetMenuItemIngredients3DAsync(
            menuItemId,
            cancellationToken));

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
