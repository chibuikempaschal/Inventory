using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StockPilot.Domain.Entities
{
    public class UserRole
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid RoleId { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User User { get; set; } = null!;

        public Role Role { get; set; } = null!;
    }
}
