namespace StockPilot.Application.DTOs;

using System.ComponentModel.DataAnnotations;
using StockPilot.Domain.Entities;

public enum StockStatus
{
    InStock,
    LowStock,
    OutOfStock
}

public sealed record RegisterRequest
{

    [Required]
    [MaxLength(20)]
    public required string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    [EmailAddress]

    public required string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public required string Password { get; set; } = string.Empty;
}
public sealed record LoginRequest
{
    [Required]
    [MaxLength(50)]
    [EmailAddress]

    public required string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public required string Password { get; set; } = string.Empty;
}

public sealed record RegisterResponse
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
}
public sealed record LoginResponse(
    Guid UserId,
    string FullName,
    string Email,
    bool IsActive,
    string AccessToken,
    string RefreshToken);

public sealed record AuthResponse(
    Guid UserId,
    string FullName,
    string Email,
    bool IsActive,
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt);

public sealed record AuthProfile(
    Guid UserId,
    string FullName,
    string Username,
    string Email,
    List<UserRoleDto> Roles,
    bool IsActive);
public sealed record UserResponse(
    Guid UserId,
    string FullName,
    string Username,
    string Email,
    List<UserRoleDto> Roles,
    bool IsActive);

public sealed record UserRoleDto(
string Role,
string? Description
);
public sealed record GetAllUserResponse(
    Guid UserId,
    string FullName,
    // string Username,
    string Email,
   List<UserRoleDto> Roles,
    bool IsActive);
public sealed record UpdateUser(
    string FullName,
    string Username,
    string Email);

public record RoleAssignmentResponse(
      List<string> PreviousRoles,
      List<string> CurrentRoles,
      string AssignedRole,
      string? RoleDescription
  );
public record CreateProductRequest(
        [Required]
        [MaxLength(50)]
        string Sku,

        [Required]
        [MaxLength(100)]
        string Name,

        [MaxLength(500)]
        string? Description,

        // The category this product belongs to
        [Required]
         [MaxLength(100)]
        string CategoryName, //Electronics, Foods etc

        // How much it costs the business
        [Range(0.01, 1000000)]
        decimal CostPrice,

        // How much it sells for
        [Range(0.01, 1000000)]
        decimal SellingPrice,

        [Range(0, int.MaxValue)]
        int QuantityInStock,

        [Range(0, int.MaxValue)]
        int ReorderLevel = 0,

        [Url]
        [MaxLength(500)]
        string? ImageUrl = null
    );

public record CreateProductResponse(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    Guid CategoryId,
    decimal SellingPrice,
    int QuantityInStock,
    string? ImageUrl,
    bool IsActive,
    DateTime CreatedAt
);
public record ProductResponse(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    Guid CategoryId,
    decimal SellingPrice,
    string? ImageUrl,
    bool IsActive,
    DateTime CreatedAt
);
public record AdminProductResponse(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    Guid CategoryId,
    decimal SellingPrice,
    int QuantityInStock,
    int ReorderLevel,
    StockStatus StockStatus,
    string? ImageUrl,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
public record LowStockProductResponse(
    string Sku,
    string Name,
    string? Description,
    int QuantityInStock,
    int ReorderLevel,
    StockStatus StockStatus,
    string? ImageUrl,
    DateTime CreatedAt
);

public record UpdateProductRequest(
[MaxLength(50)]
        string? Sku,

[MaxLength(100)]
        string? Name,

[MaxLength(500)]
        string? Description,

 [MaxLength(100)]
string CategoryName, //Electronics, Foods etc

        // How much it costs the business
        [Range(0.01, 1000000)]
        decimal CostPrice,

        // How much it sells for
        [Range(0.01, 1000000)]
        decimal SellingPrice,

[Range(0, int.MaxValue)]
        int? QuantityInStock,

[Range(0, int.MaxValue)]
        int? ReorderLevel,

[Url]
        [MaxLength(500)]
        string? ImageUrl,

bool? IsActive // Can toggle active/inactive
);

public record UpdateProductResponse(
        string Sku,
        string Name,
        string Description,

string CategoryName, //Electronics, Foods etc

        // How much it costs the business
        decimal CostPrice,

        // How much it sells for
        decimal SellingPrice,
        int QuantityInStock,
        int ReorderLevel,
        string ImageUrl,

bool IsActive // Can toggle active/inactive
);

// Category DTOs

public record CategoryRequest(
    [Required]
    [StringLength(50, MinimumLength = 1)]
    string Name,

    [MaxLength(500)]
    string? Description
);

public record UpdateCategoryRequest(
    [StringLength(50, MinimumLength = 1)]
    string? Name,

    [MaxLength(500)]
    string? Description
);

public record CategoryResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive
);

//Sale DTOs
public record CreateSaleRequest(

        [MaxLength(500)]
        string? Notes,

        [Required]
        [MinLength(1, ErrorMessage = "A sale must contain at least one item.")]
        List<CreateSaleItemRequest> Items
);

public record SaleResponse
   (
       Guid Id,
       string ReceiptNumber,
       DateTime SaleDate,
       SaleStatus Status,
       decimal Subtotal,
       decimal DiscountAmount,
       decimal TaxAmount,
       decimal TotalAmount,
       string? Notes,
       Guid UserId
   );

public record SaleDetailsResponse
(
  Guid Id,
  string ReceiptNumber,
  DateTime SaleDate,
  SaleStatus Status,
  decimal Subtotal,
  decimal DiscountAmount,
  decimal TaxAmount,
  decimal TotalAmount,
  string? Notes,
  Guid UserId,
  IReadOnlyList<SaleItemResponse> Items
);

public record UpdateSaleRequest
(
   [MaxLength(500)]
        string? Notes
);

// SaleItem DTOs
public record CreateSaleItemRequest
  (
      [Required]
        Guid ProductId,

      [Required]
        [Range(1, int.MaxValue,
            ErrorMessage = "Quantity must be at least 1.")]
        int Quantity,

      [Range(typeof(decimal), "0", "999999999999",
            ErrorMessage = "Discount cannot be negative.")]
        decimal DiscountAmount
  );

public record SaleItemResponse
   (
        Guid Id,

        Guid SaleId,

        Guid ProductId,

        string ProductName,

        string SKU,

        int Quantity,

        decimal UnitSellingPrice,

        decimal DiscountAmount,

        decimal TaxAmount,

        decimal LineTotal
   );

public record AdminSaleItemResponse
(
    Guid Id,

    Guid SaleId,

    Guid ProductId,

    string ProductName,

    string SKU,

    int Quantity,

    decimal UnitCostPrice,

    decimal UnitSellingPrice,

    decimal DiscountAmount,

    decimal TaxAmount,

    decimal LineTotal
)
{
    public decimal Profit =>
        (UnitSellingPrice - UnitCostPrice) * Quantity
        - DiscountAmount;
};

//Roles
public record RoleWithCountDto(
    Guid Id,
    string Name,
    string? Description,
    int UserRoles
);

// Business Dto
public record CreateBusinessRequest(
        [Required]
    [StringLength(150, MinimumLength = 2)]
     string Name,

    [MaxLength(1000)]
     string? Description,

    [EmailAddress]
    [MaxLength(254)]
     string? Email,

    [Phone]
    [MaxLength(30)]
     string? PhoneNumber,

    [MaxLength(300)]
     string? Address,

    [MaxLength(100)]
     string? City,

    [MaxLength(100)]
     string? State,

    [Required]
    [RegularExpression("^[A-Za-z]{2}$")]
     string CountryCode,

    [Required]
    [RegularExpression("^[A-Za-z]{3}$")]
     string CurrencyCode,

    [MaxLength(100)]
     string? TimeZoneId,

    [Url]
    [MaxLength(500)]
     string? LogoUrl
);

public record UpdateBusinessRequest(
      [StringLength(150, MinimumLength = 2)]
    string? Name,

    [MaxLength(1000)]
    string? Description,

    [EmailAddress]
    [MaxLength(254)]
    string? Email,

    [Phone]
    [MaxLength(30)]
    string? PhoneNumber,

    [MaxLength(300)]
    string? Address,

    [MaxLength(100)]
    string? City,

    [MaxLength(100)]
    string? State,

    [RegularExpression("^[A-Za-z]{2}$")]
    string? CountryCode,

    [RegularExpression("^[A-Za-z]{3}$")]
    string? CurrencyCode,

    [MaxLength(100)]
    string? TimeZoneId,

    [Url]
    [MaxLength(500)]
    string? LogoUrl
);

public record BusinessResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? Email,
    string? PhoneNumber,
    string? Address,
    string? City,
    string? State,
    string CountryCode,
    string CurrencyCode,
    string? TimeZoneId,
    string? LogoUrl,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string YourRole
);

public record MyBusinessResponse(
    Guid BusinessId,
    string Name,
    string Slug,
    string CurrencyCode,
    bool IsActive,
    string YourRole,
    DateTime JoinedAt
);

public sealed record BusinessMemberRequest(
    string Email,
    string Role);

public sealed record UpdateBusinessMemberRoleRequest(
    string Role);

public sealed record BusinessMemberResponse(
    Guid MembershipId,
    Guid UserId,
    string FullName,
    string Email,
    string Role,
    bool IsActive,
    DateTime JoinedAt);