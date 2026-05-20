namespace FinanceTracker.Models.Transaction
{
    public class PaymentMethodItem
    {
        public int Id { get; set; }
        public string MethodName { get; set; } = string.Empty;
    }

    public class TransactionItem
    {
        public int Id { get; set; }
        public Guid PublicId { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime TransactionDate { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string ExpenseName { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
    }

    public class CreateTransactionRequest
    {
        public int ExpenseTypeId { get; set; }
        public int? AccountId { get; set; }
        public int? CreditCardId { get; set; }
        public int PaymentMethodId { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime TransactionDate { get; set; }
    }

    public class UpdateTransactionRequest
    {
        public Guid PublicId { get; set; }
        public int ExpenseTypeId { get; set; }
        public int? AccountId { get; set; }
        public int? CreditCardId { get; set; }
        public int PaymentMethodId { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}