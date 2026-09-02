namespace FinanceTracker.Models.CheckIn
{
    public class CheckInStatusRow
    {
        public string QuestionType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ReferenceId { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal? ExistingValue { get; set; }
        public int SortGroup { get; set; }
    }

    public class CheckInQuestion
    {
        public string QuestionType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ReferenceId { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string QuestionText { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal? ExistingValue { get; set; }
    }

    public class CheckInMonth
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string Label { get; set; } = string.Empty;
        public bool HasPending { get; set; }
        public bool HasVerify { get; set; }
        public List<CheckInQuestion> Pending { get; set; } = new();
        public List<CheckInQuestion> Verify { get; set; } = new();
    }

    public class CheckInStatusResponse
    {
        public List<CheckInMonth> Months { get; set; } = new();
        public int TotalPending { get; set; }
        public int TotalVerify { get; set; }
    }
    public class CheckInSaveRequest
    {
        public string QuestionType { get; set; } = string.Empty;
        public string ReferenceId { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal Value { get; set; }
        public decimal? AmountPaid { get; set; }
    }
}