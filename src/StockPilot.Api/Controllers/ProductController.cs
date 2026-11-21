
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace StockPilot.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProductController : ControllerBase
{
    private readonly ILogger<ProductController> _logger;
    private readonly IProductService _productService;

    public ProductController(
        ILogger<ProductController> logger,
        IProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    private Guid GetBusinessId()
    {
        var businessIdClaim =
            User.FindFirstValue("BusinessId")
            ?? User.FindFirstValue("business_id");

        if (!Guid.TryParse(businessIdClaim, out var businessId))
        {
            throw new UnauthorizedAccessException(
                "A valid business context was not found for this user.");
        }

        return businessId;
    }

    [HttpPost("Create_Product")]
    [Authorize(Roles = "Owner,SystemAdmin,Manager")]
    public async Task<ActionResult<CreateProductResponse>> CreateProduct(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var businessId = GetBusinessId();

        var product = await _productService.CreateProductAsync(
            businessId,
            request,
            cancellationToken);

        return Ok(product);
    }

    [HttpGet("Get_ProductId")]
    [Authorize(Roles = "Owner,SystemAdmin,Manager")]
    public async Task<ActionResult<ProductResponse>> GetProductById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var businessId = GetBusinessId();

        var product = await _productService.GetProductByIdAsync(
            businessId,
            id,
            cancellationToken);

        return Ok(product);
    }

    [HttpGet("Get_All_Products")]
    [Authorize(Roles = "Owner,SystemAdmin,Manager,User")]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAllProducts(
        CancellationToken cancellationToken)
    {
        var businessId = GetBusinessId();

        var products = await _productService.GetAllProductsAsync(
            businessId,
            cancellationToken);

        return Ok(products);
    }

    [HttpGet("Get_All_Products_Admin")]
    [Authorize(Roles = "Owner,SystemAdmin")]
    public async Task<ActionResult<IEnumerable<AdminProductResponse>>> GetAllProductsAsAdmin(
        CancellationToken cancellationToken)
    {
        var businessId = GetBusinessId();

        var products = await _productService.GetAllProductsAsAdminAsync(
            businessId,
            cancellationToken);

        return Ok(products);
    }

    [HttpGet("Get_ProductName")]
    [Authorize(Roles = "Owner,SystemAdmin,Manager")]
    public async Task<ActionResult<ProductResponse>> GetProductByName(
        string name,
        CancellationToken cancellationToken)
    {
        var businessId = GetBusinessId();

        var product = await _productService.GetProductByNameAsync(
            businessId,
            name,
            cancellationToken);

        return Ok(product);
    }

    [HttpPut("Update_Product")]
    [Authorize(Roles = "Owner,SystemAdmin,Manager")]
    public async Task<ActionResult<AdminProductResponse>> UpdateProduct(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var businessId = GetBusinessId();

        var product = await _productService.UpdateProductAsync(
            businessId,
            id,
            request,
            cancellationToken);

        return Ok(product);
    }

    [HttpGet("Get_Products_By_Category")]
    [Authorize(Roles = "Owner,SystemAdmin,Manager,User")]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetProductsWithinCategory(
        string categoryName,
        CancellationToken cancellationToken)
    {
        var businessId = GetBusinessId();

        var products = await _productService.GettingAllProductsWithinCategoryAsync(
            businessId,
            categoryName,
            cancellationToken);

        return Ok(products);
    }

    [HttpGet("Get_Low_Stock_Products")]
    [Authorize(Roles = "Owner,SystemAdmin,Manager")]
    public async Task<ActionResult<IEnumerable<LowStockProductResponse>>> GetLowStockProducts(
        CancellationToken cancellationToken)
    {
        var businessId = GetBusinessId();

        var products = await _productService.GetLowStockProductsAsync(
            businessId,
            cancellationToken);

        return Ok(products);
    }

    [HttpDelete("Delete_Product")]
    [Authorize(Roles = "Owner,SystemAdmin,Manager")]
    public async Task<IActionResult> DeleteProduct(
        Guid id,
        CancellationToken cancellationToken)
    {
        var businessId = GetBusinessId();

        await _productService.DeleteProductAsync(
            businessId,
            id,
            cancellationToken);

        return NoContent();
    }
}
