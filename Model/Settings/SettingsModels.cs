namespace FinanceTracker.Models.Settings
{
    public class BankItem
    {
        public int Id { get; set; }
        public string BankName { get; set; } = string.Empty;
    }

    public class AccountTypeItem
    {
        public int Id { get; set; }
        public string TypeName { get; set; } = string.Empty;
    }

    public class UserAccountItem
    {
        public int Id { get; set; }
        public Guid PublicId { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public decimal OpeningBalance { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateUserAccountRequest
    {
        public int BankId { get; set; }
        public int AccountTypeId { get; set; }
        public decimal OpeningBalance { get; set; }
    }

    public class UpdateUserAccountRequest
    {
        public Guid PublicId { get; set; }
        public int BankId { get; set; }
        public int AccountTypeId { get; set; }
        public decimal OpeningBalance { get; set; }
    }
}