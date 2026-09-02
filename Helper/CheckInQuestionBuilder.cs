namespace FinanceTracker.Helper
{
    public static class CheckInQuestionBuilder
    {
        private static readonly string[] MonthNames = {
            "", "January", "February", "March", "April",
            "May", "June", "July", "August", "September",
            "October", "November", "December"
        };

        public static string BuildPendingQuestion(
            string questionType, string label, int month, int year)
        {
            var m = MonthNames[month];
            return questionType switch
            {
                "deposit" => $"How much did you deposit into {label} in {m} {year}?",
                "withdrawal" => $"How much did you withdraw from {label} in {m} {year}?",
                "salary" => $"How much did you earn from {label} in {m} {year}?",
                "creditcard" => $"What was your {label} bill for {m} {year}?",
                "recurring" => $"How much did you pay for {label} in {m} {year}?",
                _ => $"Please enter the amount for {label} in {m} {year}."
            };
        }

        public static string BuildVerifyQuestion(
            string questionType, string label,
            int month, int year, decimal existingValue)
        {
            var m = MonthNames[month];
            var amount = $"${existingValue:N2}";
            return questionType switch
            {
                "deposit" => $"Is your {label} deposit for {m} {year} still {amount}?",
                "withdrawal" => $"Is your {label} withdrawal for {m} {year} still {amount}?",
                "salary" => $"Is your total salary from {label} for {m} {year} still {amount}?",
                "creditcard" => $"Is your {label} bill for {m} {year} still {amount}?",
                "recurring" => $"Is your {label} expense for {m} {year} still {amount}?",
                _ => $"Is the amount for {label} in {m} {year} still {amount}?"
            };
        }
    }
}