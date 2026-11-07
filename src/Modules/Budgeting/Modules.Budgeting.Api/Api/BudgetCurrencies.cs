namespace Modules.Budgeting.Api.Api;

/// <summary>
/// Currency codes other modules may pass to <see cref="IBudgetingApi"/>, so they don't need the Budgeting
/// domain's <c>Currency</c> type.
/// </summary>
public static class BudgetCurrencies
{
    /// <summary>The currency every new budget starts in (ISO 4217 code).</summary>
    public const string Default = "USD";
}
