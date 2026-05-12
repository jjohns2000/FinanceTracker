namespace FinanceTracker.Models.CreditCard
{
    public class CreditCardItem
    {
        public int Id { get; set; }
        public Guid PublicId { get; set; }
        public string CardName { get; set; } = string.Empty;
        public decimal CreditLimit { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class MonthlyCreditCardSummaryItem
    {
        public int Id { get; set; }
        public Guid PublicId { get; set; }
        public int CreditCardId { get; set; }
        public string CardName { get; set; } = string.Empty;
        public decimal CreditLimit { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal BillAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal Balance { get; set; }
    }

    public class CreateCreditCardRequest
    {
        public string CardName { get; set; } = string.Empty;
        public decimal CreditLimit { get; set; }
        public DateTime StartDate { get; set; }
    }

    public class UpdateCreditCardRequest
    {
        public Guid PublicId { get; set; }
        public string CardName { get; set; } = string.Empty;
        public decimal CreditLimit { get; set; }
        public DateTime StartDate { get; set; }
    }

    public class UpsertMonthlyCreditCardRequest
    {
        public int CreditCardId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal BillAmount { get; set; }
        public decimal AmountPaid { get; set; }
    }
}