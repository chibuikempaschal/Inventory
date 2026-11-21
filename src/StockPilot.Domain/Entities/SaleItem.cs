using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockPilot.Domain.Entities
{
    public class SaleItem
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public int Quantity { get; set; }

        // Prices are snapshots taken when the sale occurs.
        public decimal UnitCostPrice { get; set; }

        public decimal UnitSellingPrice { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal LineTotal { get; set; }

        // Foreign keys
        public Guid SaleId { get; set; }

        public Guid ProductId { get; set; }

        // Navigation properties
        public Sale Sale { get; set; } = null!;

        public Product Product { get; set; } = null!;
    }
}