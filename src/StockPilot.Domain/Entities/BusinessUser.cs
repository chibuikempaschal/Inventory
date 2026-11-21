
namespace StockPilot.Domain.Entities;

public class BusinessUser
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid BusinessId { get; set; }

    public Guid UserId { get; set; }

    public Guid RoleId { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public Business Business { get; set; } = null!;

    public User User { get; set; } = null!;

    public Role Role { get; set; } = null!;
}
