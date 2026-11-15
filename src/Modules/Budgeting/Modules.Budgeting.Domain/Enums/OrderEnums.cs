namespace Modules.Budgeting.Domain.Enums;

public enum OrderSide
{
    Buy = 1,
    Sell = 2,
}

public enum OrderType
{
    Limit = 1,
    Stop = 2,
}

public enum OrderStatus
{
    Pending = 1,
    Filled = 2,
    Cancelled = 3,
    Rejected = 4,
}
