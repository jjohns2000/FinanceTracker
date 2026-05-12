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

    public class MonthlyTrendItem
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal TotalDeposit { get; set; }
        public decimal TotalWithdrawal { get; set; }
    }

    public class PieDataItem
    {
        public string Label { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}