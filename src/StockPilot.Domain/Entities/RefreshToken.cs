using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockPilot.Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public string TokenHash { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? ReplacedByTokenHash { get; set; }
    }
}