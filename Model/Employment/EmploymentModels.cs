namespace FinanceTracker.Models.Employment
{
    public class EmploymentTypeItem
    {
        public int Id { get; set; }
        public string TypeName { get; set; } = string.Empty;
    }

    public class PayFrequencyItem
    {
        public int Id { get; set; }
        public string FrequencyName { get; set; } = string.Empty;
    }

    public class EmploymentItem
    {
        public int Id { get; set; }
        public Guid PublicId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string JobRole { get; set; } = string.Empty;
        public string EmploymentType { get; set; } = string.Empty;
        public string PayFrequency { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class MonthlySalaryItem
    {
        public int Id { get; set; }
        public Guid PublicId { get; set; }
        public int EmploymentId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string JobRole { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal NetPay { get; set; }
    }

    public class CreateEmploymentRequest
    {
        public string CompanyName { get; set; } = string.Empty;
        public string JobRole { get; set; } = string.Empty;
        public int EmploymentTypeId { get; set; }
        public int PayFrequencyId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class UpdateEmploymentRequest
    {
        public Guid PublicId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string JobRole { get; set; } = string.Empty;
        public int EmploymentTypeId { get; set; }
        public int PayFrequencyId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class UpsertMonthlySalaryRequest
    {
        public Guid? PublicId { get; set; }
        public int EmploymentId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal NetPay { get; set; }
    }
}