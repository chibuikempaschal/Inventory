using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using StockPilot.Domain.Enums;

namespace StockPilot.Domain.Entities
{
    public class Sale
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string ReceiptNumber { get; set; } = string.Empty;

        public Guid BusinessId { get; set; }

        public Business Business { get; set; } = null!;

        public DateTime SaleDate { get; set; } = DateTime.UtcNow;

        public SaleStatus Status { get; set; } = SaleStatus.Pending;

        public decimal Subtotal { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal TotalAmount { get; set; }

        public string? Notes { get; set; }

        // Cashier who processed the sale
        public Guid UserId { get; set; }

        public User User { get; set; } = null!;

        // Products on the receipt
        public ICollection<SaleItem> SaleItems { get; set; }
            = new List<SaleItem>();

        // Payments for this sale
        public ICollection<Payment> Payments { get; set; }
            = new List<Payment>();

        // Stock history associated with this sale
        public ICollection<StockMovement> StockMovements { get; set; }
            = new List<StockMovement>();
    }
}