namespace FinanceTracker.Models.Expense
{
    public class ExpenseTypeItem
    {
        public int Id { get; set; }
        public Guid PublicId { get; set; }
        public string ExpenseName { get; set; } = string.Empty;
        public bool IsRecurring { get; set; }
        public decimal Amount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsAnnual { get; set; }
        public bool IsActive { get; set; }
        public bool IsSystemManaged { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class MonthlyExpenseEntryItem
    {
        public int Id { get; set; }
        public Guid PublicId { get; set; }
        public int ExpenseTypeId { get; set; }
        public string ExpenseName { get; set; } = string.Empty;
        public bool IsRecurring { get; set; }
        public decimal DefaultAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }

    public class CreateExpenseTypeRequest
    {
        public string ExpenseName { get; set; } = string.Empty;
        public bool IsRecurring { get; set; }
        public decimal Amount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsAnnual { get; set; }
    }

    public class UpdateExpenseTypeRequest
    {
        public Guid PublicId { get; set; }
        public string ExpenseName { get; set; } = string.Empty;
        public bool IsRecurring { get; set; }
        public decimal Amount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsAnnual { get; set; }
    }

    public class UpsertMonthlyExpenseEntryRequest
    {
        public Guid? PublicId { get; set; }
        public int ExpenseTypeId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal ActualAmount { get; set; }
    }
}