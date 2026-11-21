
namespace StockPilot.Domain.Entities;

public class Business
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    // URL-friendly unique identifier, e.g. baylon-store-a81f2c
    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    // ISO 3166-1 alpha-2, e.g. NG
    public string CountryCode { get; set; } = "NG";

    // ISO 4217, e.g. NGN, USD, GBP
    public string CurrencyCode { get; set; } = "NGN";

    public string? TimeZoneId { get; set; }

    public string? LogoUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ICollection<BusinessUser> Members { get; set; }
        = new List<BusinessUser>();
}
