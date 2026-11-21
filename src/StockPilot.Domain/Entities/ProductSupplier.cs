using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockPilot.Domain.Entities
{
    public class ProductSupplier
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProductId { get; set; }
        public Guid SupplierId { get; set; }
        public string? SupplierProductCode { get; set; }

        public decimal? LastPurchasePrice { get; set; }

        public int? LeadTimeDays { get; set; }

        // Navigation properties
        public Product Product { get; set; } = null!;

        public Supplier Supplier { get; set; } = null!;

    }
}