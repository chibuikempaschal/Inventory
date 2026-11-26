using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using StockPilot.Application.Interfaces.Services;

namespace StockPilot.Api.Controllers;

[ApiController]
[Route("api/businesses/{businessId:guid}/members")]
[Authorize]
public sealed class BusinessMemberController(
    IBusinessMemberService businessMemberService) : ControllerBase
{
    // GET: api/businesses/{businessId}/members
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BusinessMemberResponse>>> GetMembers(
        Guid businessId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        var members = await businessMemberService.GetMembersAsync(
            userId,
            businessId,
            cancellationToken);

        return Ok(members);
    }

    // POST: api/businesses/{businessId}/members
    [HttpPost]
    public async Task<ActionResult<BusinessMemberResponse>> AddMember(
        Guid businessId,
        [FromBody] BusinessMemberRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        var member = await businessMemberService.AddMemberAsync(
            userId,
            businessId,
            request,
            cancellationToken);

        return Ok(member);
    }

    // PUT: api/businesses/{businessId}/members/{targetUserId}/role
    [HttpPut("{targetUserId:guid}/role")]
    public async Task<ActionResult<BusinessMemberResponse>> UpdateMemberRole(
        Guid businessId,
        Guid targetUserId,
        [FromBody] UpdateBusinessMemberRoleRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        var member = await businessMemberService.UpdateMemberRoleAsync(
            userId,
            businessId,
            targetUserId,
            request,
            cancellationToken);

        return Ok(member);
    }

    // DELETE: api/businesses/{businessId}/members/{targetUserId}
    [HttpDelete("{targetUserId:guid}")]
    public async Task<IActionResult> RemoveMember(
        Guid businessId,
        Guid targetUserId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        await businessMemberService.RemoveMemberAsync(
            userId,
            businessId,
            targetUserId,
            cancellationToken);

        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException(
                "The authenticated user's ID is missing or invalid.");
        }

        return userId;
    }
}