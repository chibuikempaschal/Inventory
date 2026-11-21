
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockPilot.Domain.Constants;

namespace StockPilot.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public sealed class CategoryController(
    ICategoryService categoryService,
    ILogger<CategoryController> logger) : ControllerBase
{
    private Guid GetBusinessId()
    {
        var businessIdClaim =
            User.FindFirstValue("BusinessId")
            ?? User.FindFirstValue("business_id");

        if (!Guid.TryParse(businessIdClaim, out var businessId) ||
            businessId == Guid.Empty)
        {
            logger.LogWarning(
                "Authenticated user has no valid BusinessId claim.");

            throw new UnauthorizedAccessException(
                "A valid business context was not found for this user.");
        }

        return businessId;
    }

    // ---------------------------------------------------------
    // CREATE CATEGORY
    // ---------------------------------------------------------

    [HttpPost("Create_Category")]
    [Authorize(Roles = "Owner,SystemAdmin,Manager")]
    public async Task<ActionResult<CategoryResponse>> CreateCategory(
        [FromBody] CategoryRequest request,
        CancellationToken cancellationToken)
    {
        var businessId = GetBusinessId();

        var category = await categoryService.CreateCategory(
            businessId,
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetCategoryById),
            new { id = category.Id },
            category);
    }

    // ---------------------------------------------------------
    // GET CATEGORY BY ID
    // ---------------------------------------------------------

    [HttpGet("Get_Category/{id:guid}")]
    [Authorize(Roles = "Owner,SystemAdmin,Manager,User")]
    public async Task<ActionResult<CategoryResponse>> GetCategoryById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var businessId = GetBusinessId();

        var category = await categoryService.GetCategoryById(
            businessId,
            id,
            cancellationToken);

        return Ok(category);
    }

    // ---------------------------------------------------------
    // GET ALL CATEGORIES
    // ---------------------------------------------------------

    [HttpGet("Get_All_Category")]
    [Authorize(Roles = "Owner,SystemAdmin,Manager,User")]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> GetAllCategories(
        CancellationToken cancellationToken)
    {
        var businessId = GetBusinessId();

        var categories = await categoryService.GetAllCategory(
            businessId,
            cancellationToken);

        return Ok(categories);
    }

    // ---------------------------------------------------------
    // UPDATE CATEGORY
    // ---------------------------------------------------------

    [HttpPut("Update_Category/{id:guid}")]
    [Authorize(Roles = "Owner,SystemAdmin,Manager")]
    public async Task<ActionResult<CategoryResponse>> UpdateCategory(
        Guid id,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var businessId = GetBusinessId();

        var category = await categoryService.UpdateCategory(
            businessId,
            id,
            request,
            cancellationToken);

        return Ok(category);
    }

    // ---------------------------------------------------------
    // DELETE CATEGORY (SOFT DELETE)
    // ---------------------------------------------------------

    [HttpDelete("Delete_Category/{id:guid}")]
    [Authorize(Roles = "Owner,SystemAdmin,Manager")]
    public async Task<IActionResult> DeleteCategory(
        Guid id,
        CancellationToken cancellationToken)
    {
        var businessId = GetBusinessId();

        await categoryService.DeleteCategory(
            businessId,
            id,
            cancellationToken);

        return NoContent();
    }
}
