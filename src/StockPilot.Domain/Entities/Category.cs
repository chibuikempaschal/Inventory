using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StockPilot.Domain.Entities
{
    public class Category
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; } = string.Empty;

        public Guid BusinessId { get; set; }
        public Business Business { get; set; } = null!;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Product> Products { get; set; }
            = new List<Product>();
    }
}