using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using StockPilot.Domain.Enums;

namespace StockPilot.Domain.Entities
{
    public class Payment
    {
        public Guid Id { get; private set; } = Guid.NewGuid();

        public decimal Amount { get; set; }

        public PaymentMethod Method { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public string? TransactionReference { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        public string? Notes { get; set; }

        // Foreign key
        public Guid SaleId { get; set; }

        public Sale Sale { get; set; } = null!;
    }
}
