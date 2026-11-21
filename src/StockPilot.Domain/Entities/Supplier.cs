using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StockPilot.Domain.Entities
{
    public class Supplier
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? ContactPerson { get; set; }

        public Guid BusinessId { get; set; }

        public Business Business { get; set; } = null!;

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ProductSupplier> ProductSuppliers { get; set; }
            = new List<ProductSupplier>();
    }
}