namespace Modules.Budgeting.Domain.Enums;

public enum AuditLogType
{
    BuyStock = 0,
    SellStock = 1,
    CancelOrder = 2,
    ModifyOrder = 3,
    DividendReceived = 4,
    AccountCredit = 5,
    AccountDebit = 6,
    Login = 7,
    Logout = 8,
}
