namespace FinanceTracker.Models.Income
{
    public class MonthlyAccountSummaryItem
    {
        public Guid PublicId { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal Deposit { get; set; }
        public decimal Interest { get; set; }
        public decimal Withdrawal { get; set; }
        public decimal ClosingBalance { get; set; }
        public decimal Trend { get; set; }
    }

    public class MonthlyAggregateItem
    {
        public decimal TotalOpeningBalance { get; set; }
        public decimal TotalDeposit { get; set; }
        public decimal TotalInterest { get; set; }
        public decimal TotalWithdrawal { get; set; }
        public decimal TotalClosingBalance { get; set; }
        public decimal TotalTrend { get; set; }
    }

    public class UpsertDepositRequest
    {
        public Guid AccountPublicId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal Deposit { get; set; }
        public decimal Interest { get; set; }
    }

    public class UpsertWithdrawalRequest
    {
        public Guid AccountPublicId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal Withdrawal { get; set; }
    }

    public class GetMonthlyRequest
    {
        public int Month { get; set; }
        public int Year { get; set; }
    }
    public class AccountRunningTotal
    {
        public Guid AccountPublicId { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public decimal InitialOpeningBalance { get; set; }
        public decimal TotalDeposits { get; set; }
        public decimal TotalInterest { get; set; }
        public decimal TotalWithdrawals { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal BestTrend { get; set; }
        public int MonthsTracked { get; set; }
    }
}