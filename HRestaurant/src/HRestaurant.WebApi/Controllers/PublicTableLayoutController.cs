using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.Table;
using HRestaurant.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRestaurant.Controllers;

[AllowAnonymous]
[Produces("application/json")]
[Route("api/public/branches/{branchId:guid}/tables/layout")]
public sealed class PublicTableLayoutController : ApiControllerBase
{
    private readonly ITableService _service;
    public PublicTableLayoutController(ITableService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<PublicTableLayoutDTO>>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid branchId, CancellationToken cancellationToken) =>
        FromResponse(await _service.GetPublicLayoutAsync(branchId, cancellationToken));
}
