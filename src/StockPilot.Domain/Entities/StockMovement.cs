using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using StockPilot.Domain.Enums;

namespace StockPilot.Domain.Entities
{
    public class StockMovement
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public StockMovementType Type { get; set; }

        // Positive for stock received; negative for stock removed.
        public int QuantityChange { get; set; }

        public int QuantityBefore { get; set; }

        public int QuantityAfter { get; set; }

        public string? Reason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Product affected
        public Guid ProductId { get; set; }

        public Product Product { get; set; } = null!;

        // User who performed the action
        public Guid UserId { get; set; }

        public User User { get; set; } = null!;

        // Optional link to the sale that caused this movement
        public Guid? SaleId { get; set; }

        public Sale? Sale { get; set; }
    }
}