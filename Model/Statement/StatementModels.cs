namespace FinanceTracker.Models.Statement
{
    public class ExtractedTransaction
    {
        public string Date { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? SuggestedCategory { get; set; }
    }

    public class StatementExtractResponse
    {
        public string StatementType { get; set; } = string.Empty;
        public List<ExtractedTransaction> Transactions { get; set; } = new();
    }

    public class ImportTransaction
    {
        public string Date { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Type { get; set; } = string.Empty;
        public int? ExpenseTypeId { get; set; }
        public string? CreditType { get; set; }  // "deposit" | "salary:{id}" | "skip"
        public int? EmploymentId { get; set; }  // populated when creditType = salary
    }

    public class StatementImportRequest
    {
        public string StatementType { get; set; } = string.Empty; // "bank" | "creditcard"
        public string? AccountPublicId { get; set; }   // bank account
        public string? CreditCardPublicId { get; set; }   // credit card
        public List<ImportTransaction> Transactions { get; set; } = new();
    }

    public class DuplicateCheckRequest
    {
        public string? AccountPublicId { get; set; }
        public List<ImportTransaction> Transactions { get; set; } = new();
    }

    public class StatementImportResult
    {
        public int InsertedTransactions { get; set; }
        public int MonthsAffected { get; set; }
    }
}