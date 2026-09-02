using Dapper;
using FinanceTracker.Data;
using FinanceTracker.Helper;
using FinanceTracker.Models.CheckIn;
using System.Text.Json;

namespace FinanceTracker.Services
{
    public interface ICheckInService
    {
        Task<CheckInStatusResponse> GetCheckInStatus(Guid publicId);
        Task SaveAnswer(Guid publicId, CheckInSaveRequest request);
    }

    public class CheckInService : ICheckInService
    {
        private readonly DbContext _db;

        private static readonly string[] MonthNames = {
            "", "January", "February", "March", "April",
            "May", "June", "July", "August", "September",
            "October", "November", "December"
        };

        public CheckInService(DbContext db)
        {
            _db = db;
        }

        private async Task<int> GetUserId(Guid publicId)
        {
            using var connection = _db.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<int>(
                "sp_GetUserByPublicId",
                new { PublicId = publicId.ToString() },
                commandType: System.Data.CommandType.StoredProcedure
            );
        }

        public async Task<CheckInStatusResponse> GetCheckInStatus(Guid publicId)
        {
            using var connection = _db.CreateConnection();
            var userId = await GetUserId(publicId);

            var p = new DynamicParameters();
            p.Add("@UserId", userId);

            var rows = (await connection.QueryAsync<CheckInStatusRow>(
                "sp_GetCheckInStatus", p,
                commandType: System.Data.CommandType.StoredProcedure
            )).ToList();

            var monthGroups = rows
                .GroupBy(r => new { r.Month, r.Year })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .ToList();

            var months = new List<CheckInMonth>();

            foreach (var group in monthGroups)
            {
                var monthLabel = $"{MonthNames[group.Key.Month]} {group.Key.Year}";

                var pending = group
                    .Where(r => r.Status == "pending")
                    .Select(r => new CheckInQuestion
                    {
                        QuestionType = r.QuestionType,
                        Status = r.Status,
                        ReferenceId = r.ReferenceId,
                        Label = r.Label,
                        Month = r.Month,
                        Year = r.Year,
                        ExistingValue = r.ExistingValue,
                        QuestionText = CheckInQuestionBuilder.BuildPendingQuestion(
                            r.QuestionType, r.Label, r.Month, r.Year)
                    })
                    .ToList();

                var verify = group
                    .Where(r => r.Status == "verify")
                    .Select(r => new CheckInQuestion
                    {
                        QuestionType = r.QuestionType,
                        Status = r.Status,
                        ReferenceId = r.ReferenceId,
                        Label = r.Label,
                        Month = r.Month,
                        Year = r.Year,
                        ExistingValue = r.ExistingValue,
                        QuestionText = CheckInQuestionBuilder.BuildVerifyQuestion(
                            r.QuestionType, r.Label, r.Month, r.Year,
                            r.ExistingValue ?? 0)
                    })
                    .ToList();

                months.Add(new CheckInMonth
                {
                    Month = group.Key.Month,
                    Year = group.Key.Year,
                    Label = monthLabel,
                    HasPending = pending.Count > 0,
                    HasVerify = verify.Count > 0,
                    Pending = pending,
                    Verify = verify
                });
            }

            return new CheckInStatusResponse
            {
                Months = months,
                TotalPending = months.Sum(m => m.Pending.Count),
                TotalVerify = months.Sum(m => m.Verify.Count)
            };
        }

        public async Task SaveAnswer(Guid publicId, CheckInSaveRequest request)
        {
            using var connection = _db.CreateConnection();
            var userId = await GetUserId(publicId);

            switch (request.QuestionType.ToLower())
            {
                case "deposit":
                    {
                        var accountId = await connection.QueryFirstOrDefaultAsync<int>(
                            "SELECT Id FROM UserAccounts WHERE PublicId = @PublicId AND UserId = @UserId",
                            new { PublicId = request.ReferenceId, UserId = userId });

                        if (accountId == 0)
                            throw new Exception("Account not found.");

                        var p = new DynamicParameters();
                        p.Add("@JsonData", JsonSerializer.Serialize(new
                        {
                            UserId = userId,
                            AccountId = accountId,
                            request.Month,
                            request.Year,
                            Deposit = request.Value,
                            Interest = 0,
                            IsTransfer = false      // check-in answers are never transfers
                        }));
                        await connection.ExecuteAsync(
                            "sp_UpsertMonthlyDeposit", p,
                            commandType: System.Data.CommandType.StoredProcedure);
                        break;
                    }

                case "withdrawal":
                    {
                        var accountId = await connection.QueryFirstOrDefaultAsync<int>(
                            "SELECT Id FROM UserAccounts WHERE PublicId = @PublicId AND UserId = @UserId",
                            new { PublicId = request.ReferenceId, UserId = userId });

                        if (accountId == 0)
                            throw new Exception("Account not found.");

                        var p = new DynamicParameters();
                        p.Add("@JsonData", JsonSerializer.Serialize(new
                        {
                            UserId = userId,
                            AccountId = accountId,
                            request.Month,
                            request.Year,
                            Withdrawal = request.Value,
                            IsTransfer = false      // check-in answers are never transfers
                        }));
                        await connection.ExecuteAsync(
                            "sp_UpsertMonthlyWithdrawal", p,
                            commandType: System.Data.CommandType.StoredProcedure);
                        break;
                    }

                case "salary":
                    {
                        var employmentId = await connection.QueryFirstOrDefaultAsync<int>(
                            "SELECT Id FROM Employment WHERE PublicId = @PublicId AND UserId = @UserId",
                            new { PublicId = request.ReferenceId, UserId = userId });

                        if (employmentId == 0)
                            throw new Exception("Employment not found.");

                        var p = new DynamicParameters();
                        p.Add("@JsonData", JsonSerializer.Serialize(new
                        {
                            UserId = userId,
                            EmploymentId = employmentId,
                            request.Month,
                            request.Year,
                            NetPay = request.Value
                        }));
                        await connection.ExecuteAsync(
                            "sp_UpsertMonthlySalary", p,
                            commandType: System.Data.CommandType.StoredProcedure);
                        break;
                    }

                case "creditcard":
                    {
                        var creditCardId = await connection.QueryFirstOrDefaultAsync<int>(
                            "SELECT Id FROM CreditCards WHERE PublicId = @PublicId AND UserId = @UserId",
                            new { PublicId = request.ReferenceId, UserId = userId });

                        if (creditCardId == 0)
                            throw new Exception("Credit card not found.");

                        var p = new DynamicParameters();
                        p.Add("@JsonData", JsonSerializer.Serialize(new
                        {
                            UserId = userId,
                            CreditCardId = creditCardId,
                            request.Month,
                            request.Year,
                            BillAmount = request.Value,
                            AmountPaid = request.AmountPaid ?? 0
                        }));
                        await connection.ExecuteAsync(
                            "sp_UpsertMonthlyCreditCardSummary", p,
                            commandType: System.Data.CommandType.StoredProcedure);
                        break;
                    }

                case "recurring":
                    {
                        var expenseTypeId = await connection.QueryFirstOrDefaultAsync<int>(
                            "SELECT Id FROM ExpenseTypes WHERE PublicId = @PublicId AND UserId = @UserId",
                            new { PublicId = request.ReferenceId, UserId = userId });

                        if (expenseTypeId == 0)
                            throw new Exception("Expense type not found.");

                        var p = new DynamicParameters();
                        p.Add("@JsonData", JsonSerializer.Serialize(new
                        {
                            UserId = userId,
                            ExpenseTypeId = expenseTypeId,
                            request.Month,
                            request.Year,
                            ActualAmount = request.Value
                        }));
                        await connection.ExecuteAsync(
                            "sp_UpsertMonthlyExpenseEntry", p,
                            commandType: System.Data.CommandType.StoredProcedure);
                        break;
                    }
            }
        }
    }
}