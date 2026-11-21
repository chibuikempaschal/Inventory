
namespace StockPilot.Domain.Entities;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Business ownership
    public Guid BusinessId { get; set; }
    public Business Business { get; set; } = null!;

    // Product identification
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Pricing
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }

    // Inventory
    public int QuantityInStock { get; set; }
    public int ReorderLevel { get; set; }

    // Product image
    public string? ImageUrl { get; set; }

    // Status
    public bool IsActive { get; set; } = true;

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Category relationship
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    // Related entities
    public ICollection<ProductSupplier> ProductSuppliers { get; set; }
        = new List<ProductSupplier>();

    public ICollection<StockMovement> StockMovements { get; set; }
        = new List<StockMovement>();

    public ICollection<SaleItem> SaleItems { get; set; }
        = new List<SaleItem>();

    public ICollection<StockAlert> StockAlerts { get; set; }
        = new List<StockAlert>();
}
