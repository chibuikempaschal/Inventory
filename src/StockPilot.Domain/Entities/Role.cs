using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StockPilot.Domain.Entities
{
    public class Role
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        public ICollection<UserRole> UserRoles { get; set; }
            = new List<UserRole>();
    }
}
