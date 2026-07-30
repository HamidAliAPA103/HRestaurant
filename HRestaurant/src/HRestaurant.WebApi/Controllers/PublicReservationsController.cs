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
[Produces("application/json")]
[Route("api/public/reservations")]
public sealed class PublicReservationsController : ApiControllerBase
{
    private readonly IPublicReservationService _service;

    public PublicReservationsController(
        IPublicReservationService service)
    {
        ArgumentNullException.ThrowIfNull(service);
        _service = service;
    }

    [HttpPost]
    [EnableRateLimiting(RateLimitPolicies.ReservationCreate)]
    [SwaggerOperation(
        Summary = "Create a public reservation",
        Description =
            "Creates a guest reservation without JWT authentication. "
            + "The backend revalidates branch hours, table capacity and "
            + "availability inside a serializable transaction. Returns "
            + "409 if another request takes the table.",
        Tags = ["Public Reservations"])]
    [ProducesResponseType(
        typeof(ApiResponse<PublicReservationCreatedDto>),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status409Conflict)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(
        PublicCreateReservationDto dto,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.CreateAsync(dto, cancellationToken));
    }

    [HttpPost("lookup")]
    [EnableRateLimiting(RateLimitPolicies.ReservationLookup)]
    [SwaggerOperation(
        Summary = "Look up a public reservation",
        Description =
            "Use either confirmationCode + phone or a trackingToken. "
            + "Invalid combinations return the same generic error.",
        Tags = ["Public Reservations"])]
    [ProducesResponseType(
        typeof(ApiResponse<PublicReservationDetailsDto>),
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
    public async Task<IActionResult> Lookup(
        PublicReservationLookupRequestDto dto,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.LookupAsync(dto, cancellationToken));
    }

    [HttpGet("track/{trackingToken}")]
    [EnableRateLimiting(RateLimitPolicies.ReservationLookup)]
    [SwaggerOperation(
        Summary = "Track a public reservation",
        Description =
            "Looks up a reservation using the one-time disclosed, "
            + "high-entropy public tracking token.",
        Tags = ["Public Reservations"])]
    [ProducesResponseType(
        typeof(ApiResponse<PublicReservationDetailsDto>),
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
    public async Task<IActionResult> Track(
        string trackingToken,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.TrackAsync(
                trackingToken,
                cancellationToken));
    }

    [HttpPost("{confirmationCode}/cancel")]
    [EnableRateLimiting(RateLimitPolicies.ReservationLookup)]
    [SwaggerOperation(
        Summary = "Cancel a public reservation",
        Description =
            "Cancels an eligible Pending or Confirmed reservation using "
            + "the matching phone or tracking token. The configured "
            + "cancellation cutoff is enforced.",
        Tags = ["Public Reservations"])]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status409Conflict)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Cancel(
        string confirmationCode,
        PublicCancelReservationDto dto,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.CancelAsync(
                confirmationCode,
                dto,
                cancellationToken));
    }
}
