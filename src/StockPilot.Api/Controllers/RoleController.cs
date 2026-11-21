
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockPilot.Application.Interfaces;
using static StockPilot.Application.Common.Exceptions.ApplicationException;

namespace StockPilot.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class RoleController(IRoleService roleService) : ControllerBase
{
    [HttpPost("assign")]
    [Authorize(Roles = "Owner,SystemAdmin,Manager")]
    public async Task<IActionResult> AssignRole(
        [FromQuery] Guid userId,
        [FromQuery] string roleName,
        [FromQuery] string? description,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();

        var response = await roleService.AssignRoleAsync(
            currentUserId,
            userId,
            roleName,
            description,
            cancellationToken);

        return Ok(response);
    }

    [HttpDelete("remove")]
    [Authorize(Roles = "Owner,SystemAdmin,Manager")]
    public async Task<IActionResult> RemoveRole(
        [FromQuery] Guid userId,
        [FromQuery] string roleName,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();

        await roleService.RemoveRoleAsync(
            currentUserId,
            userId,
            roleName,
            cancellationToken);

        return NoContent();
    }

    [HttpGet("roles")]
    [Authorize(Roles = "Owner,SystemAdmin")]
    public async Task<IActionResult> GetAllRoles(
        CancellationToken cancellationToken)
    {
        var roles = await roleService.GetAllRolesAsync(
            cancellationToken);

        return Ok(roles);
    }

    [HttpGet("userRole")]
    [Authorize(Roles = "Owner,SystemAdmin,Manager,User")]
    public async Task<IActionResult> GetRoles(
        CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();

        var roles = await roleService.GetRolesAsync(
            currentUserId,
            cancellationToken);

        return Ok(roles);
    }

    private Guid GetCurrentUserId()
    {
        var userIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue("sub");

        if (!Guid.TryParse(userIdValue, out var userId) ||
            userId == Guid.Empty)
        {
            throw new ForbiddenException(
                "User identifier claim is missing or invalid.");
        }

        return userId;
    }
}
