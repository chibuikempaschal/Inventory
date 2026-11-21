
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockPilot.Domain.Constants;
using static StockPilot.Application.Common.Exceptions.ApplicationException;

namespace StockPilot.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class BusinessController(
    IBusinessService businessService) : ControllerBase
{
    private Guid GetCurrentUserId()
    {
        var userIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (!Guid.TryParse(userIdClaim, out var userId) ||
            userId == Guid.Empty)
        {
            throw new ForbiddenException(
                "The authenticated user identifier is missing or invalid.");
        }

        return userId;
    }

    // ---------------------------------------------------------
    // CREATE BUSINESS
    // Any authenticated user can create a business.
    // The creator automatically becomes its Owner.
    // ---------------------------------------------------------

    [HttpPost]
    public async Task<ActionResult<BusinessResponse>> CreateBusiness(
        [FromBody] CreateBusinessRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        var business = await businessService.CreateBusinessAsync(
            userId,
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetBusinessById),
            new { businessId = business.Id },
            business);
    }

    // ---------------------------------------------------------
    // GET MY BUSINESSES
    // Any authenticated user.
    // ---------------------------------------------------------

    [HttpGet("my-businesses")]
    public async Task<ActionResult<IReadOnlyList<MyBusinessResponse>>>
        GetMyBusinesses(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        var businesses = await businessService.GetMyBusinessesAsync(
            userId,
            cancellationToken);

        return Ok(businesses);
    }

    // ---------------------------------------------------------
    // GET ONE BUSINESS
    // Membership is verified by BusinessService.
    // ---------------------------------------------------------

    [HttpGet("{businessId:guid}")]
    public async Task<ActionResult<BusinessResponse>> GetBusinessById(
        Guid businessId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        var business = await businessService.GetBusinessByIdAsync(
            userId,
            businessId,
            cancellationToken);

        return Ok(business);
    }

    // ---------------------------------------------------------
    // UPDATE BUSINESS
    // Owner and Manager.
    // BusinessService verifies the actual membership role.
    // ---------------------------------------------------------

    [HttpPut("{businessId:guid}")]
    [Authorize(Roles = BusinessRoles.Owner + "," + BusinessRoles.Manager)]
    public async Task<ActionResult<BusinessResponse>> UpdateBusiness(
        Guid businessId,
        [FromBody] UpdateBusinessRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        var business = await businessService.UpdateBusinessAsync(
            userId,
            businessId,
            request,
            cancellationToken);

        return Ok(business);
    }

    // ---------------------------------------------------------
    // DEACTIVATE BUSINESS
    // Owner only.
    // BusinessService also verifies ownership.
    // ---------------------------------------------------------

    [HttpDelete("{businessId:guid}")]
    [Authorize(Roles = BusinessRoles.Owner)]
    public async Task<IActionResult> DeactivateBusiness(
        Guid businessId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        await businessService.DeactivateBusinessAsync(
            userId,
            businessId,
            cancellationToken);

        return NoContent();
    }
}
