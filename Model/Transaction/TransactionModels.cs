namespace FinanceTracker.Models.Transaction
{
    public class TransactionItem
    {
        public Guid PublicId { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Description { get; set; }
        public bool IsTransfer { get; set; }
        // Regular transaction fields — null for transfers
        public string? ExpenseName { get; set; }
        public string? PaymentMethod { get; set; }
        // Transfer fields — null for regular transactions
        public string? FromAccountName { get; set; }
        public string? ToAccountName { get; set; }
    }

    public class CreateTransactionRequest
    {
        public int ExpenseTypeId { get; set; }
        public int PaymentMethodId { get; set; }
        public decimal Amount { get; set; }
        public string TransactionDate { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? AccountId { get; set; }
        public int? CreditCardId { get; set; }
    }

    public class UpdateTransactionRequest
    {
        public string PublicId { get; set; } = string.Empty;
        public int ExpenseTypeId { get; set; }
        public int PaymentMethodId { get; set; }
        public decimal Amount { get; set; }
        public string TransactionDate { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? AccountId { get; set; }
        public int? CreditCardId { get; set; }
    }

    public class CreateTransferRequest
    {
        public string FromAccountPublicId { get; set; } = string.Empty;
        public string ToAccountPublicId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string TransactionDate { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}