namespace StockPilot.Domain.Enums;

public enum PaymentMethod
{
    Cash,
    BankTransfer,
    Card,
    Other
}

public enum PaymentStatus
{
    Pending,
    Completed,
    Failed,
    Refunded
}
