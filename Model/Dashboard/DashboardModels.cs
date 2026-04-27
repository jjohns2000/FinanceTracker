namespace FinanceTracker.Models.Dashboard
{
    public class KpiItem
    {
        public string Label { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }

    public class KpiData
    {
        public KpiItem TotalIncome { get; set; } = new();
        public KpiItem TotalExpenses { get; set; } = new();
        public KpiItem NetBalance { get; set; } = new();
        public KpiItem BudgetRemaining { get; set; } = new();
    }
}