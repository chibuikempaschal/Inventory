

namespace StockPilot.Domain.Entities
{
    public class User

    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public ICollection<UserRole> UserRoles { get; set; }
            = new List<UserRole>();

        public ICollection<Sale> Sales { get; set; }
            = new List<Sale>();
    }
}

