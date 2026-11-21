using StockPilot.Domain.Enums;

namespace StockPilot.Domain.Entities
{
    public class StockAlert
    {
        public Guid Id { get; private set; } = Guid.NewGuid();

        public int QuantityAtAlert { get; set; }

        public int ReorderLevelAtAlert { get; set; }

        public string Message { get; set; } = string.Empty;

        public StockAlertStatus Status { get; set; }
            = StockAlertStatus.Active;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ResolvedAt { get; set; }

        // Foreign key
        public Guid ProductId { get; set; }

        public Product Product { get; set; } = null!;
    }
}