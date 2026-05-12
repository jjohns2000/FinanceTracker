using Dapper;
using FinanceTracker.Data;
using FinanceTracker.Models.Expense;
using System.Text.Json;

namespace FinanceTracker.Services
{
    public interface IExpenseTypeService
    {
        Task<IEnumerable<ExpenseTypeItem>> GetExpenseTypes(Guid userPublicId);
        Task<IEnumerable<ExpenseTypeItem>> GetActiveExpenseTypes(Guid userPublicId, int month, int year);
        Task<int> CreateExpenseType(Guid userPublicId, CreateExpenseTypeRequest request);
        Task UpdateExpenseType(UpdateExpenseTypeRequest request);
        Task DeleteExpenseType(Guid publicId);
        Task<IEnumerable<MonthlyExpenseEntryItem>> GetMonthlyExpenseEntries(Guid userPublicId, int month, int year);
        Task<Guid> UpsertMonthlyExpenseEntry(Guid userPublicId, UpsertMonthlyExpenseEntryRequest request);
        Task DeleteMonthlyExpenseEntry(Guid publicId);
    }

    public class ExpenseTypeService : IExpenseTypeService
    {
        private readonly DbContext _db;

        public ExpenseTypeService(DbContext db)
        {
            _db = db;
        }

        private async Task<int> GetUserId(Guid userPublicId)
        {
            using var connection = _db.CreateConnection();
            var p = new DynamicParameters();
            p.Add("@PublicId", userPublicId);
            var user = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "sp_GetUserByPublicId", p,
                commandType: System.Data.CommandType.StoredProcedure
            );
            if (user == null) throw new Exception("User not found.");
            return (int)user.Id;
        }

        public async Task<IEnumerable<ExpenseTypeItem>> GetExpenseTypes(Guid userPublicId)
        {
            using var connection = _db.CreateConnection();
            var userId = await GetUserId(userPublicId);
            var p = new DynamicParameters();
            p.Add("@UserId", userId);
            return await connection.QueryAsync<ExpenseTypeItem>(
                "sp_GetExpenseTypes", p,
                commandType: System.Data.CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<ExpenseTypeItem>> GetActiveExpenseTypes(
            Guid userPublicId, int month, int year)
        {
            using var connection = _db.CreateConnection();
            var userId = await GetUserId(userPublicId);
            var p = new DynamicParameters();
            p.Add("@UserId", userId);
            p.Add("@Month", month);
            p.Add("@Year", year);
            return await connection.QueryAsync<ExpenseTypeItem>(
                "sp_GetActiveExpenseTypes", p,
                commandType: System.Data.CommandType.StoredProcedure
            );
        }

        public async Task<int> CreateExpenseType(
            Guid userPublicId, CreateExpenseTypeRequest request)
        {
            using var connection = _db.CreateConnection();
            var userId = await GetUserId(userPublicId);

            var payload = new
            {
                UserId = userId,
                request.ExpenseName,
                request.IsRecurring,
                request.Amount,
                StartDate = request.StartDate?.ToString("yyyy-MM-dd"),
                EndDate = request.EndDate?.ToString("yyyy-MM-dd"),
                request.IsAnnual
            };

            var p = new DynamicParameters();
            p.Add("@JsonData", JsonSerializer.Serialize(payload));

            var result = await connection.QueryFirstOrDefaultAsync<int>(
                "sp_CreateExpenseType", p,
                commandType: System.Data.CommandType.StoredProcedure
            );

            if (result == 0) throw new Exception("Failed to create expense type.");
            return result;
        }

        public async Task UpdateExpenseType(UpdateExpenseTypeRequest request)
        {
            using var connection = _db.CreateConnection();

            var payload = new
            {
                PublicId = request.PublicId.ToString(),
                request.ExpenseName,
                request.IsRecurring,
                request.Amount,
                StartDate = request.StartDate?.ToString("yyyy-MM-dd"),
                EndDate = request.EndDate?.ToString("yyyy-MM-dd"),
                request.IsAnnual
            };

            var p = new DynamicParameters();
            p.Add("@JsonData", JsonSerializer.Serialize(payload));

            await connection.ExecuteAsync(
                "sp_UpdateExpenseType", p,
                commandType: System.Data.CommandType.StoredProcedure
            );
        }

        public async Task DeleteExpenseType(Guid publicId)
        {
            using var connection = _db.CreateConnection();
            var p = new DynamicParameters();
            p.Add("@PublicId", publicId);
            await connection.ExecuteAsync(
                "sp_DeleteExpenseType", p,
                commandType: System.Data.CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<MonthlyExpenseEntryItem>> GetMonthlyExpenseEntries(
            Guid userPublicId, int month, int year)
        {
            using var connection = _db.CreateConnection();
            var userId = await GetUserId(userPublicId);
            var p = new DynamicParameters();
            p.Add("@UserId", userId);
            p.Add("@Month", month);
            p.Add("@Year", year);
            return await connection.QueryAsync<MonthlyExpenseEntryItem>(
                "sp_GetMonthlyExpenseEntries", p,
                commandType: System.Data.CommandType.StoredProcedure
            );
        }

        public async Task<Guid> UpsertMonthlyExpenseEntry(
            Guid userPublicId, UpsertMonthlyExpenseEntryRequest request)
        {
            using var connection = _db.CreateConnection();
            var userId = await GetUserId(userPublicId);

            var payload = new
            {
                UserId = userId,
                request.ExpenseTypeId,
                request.Month,
                request.Year,
                request.ActualAmount,
                PublicId = request.PublicId?.ToString()
            };

            var p = new DynamicParameters();
            p.Add("@JsonData", JsonSerializer.Serialize(payload));

            var result = await connection.QueryFirstOrDefaultAsync<Guid>(
                "sp_UpsertMonthlyExpenseEntry", p,
                commandType: System.Data.CommandType.StoredProcedure
            );

            if (result == Guid.Empty)
                throw new Exception("Failed to save expense entry.");

            return result;
        }

        public async Task DeleteMonthlyExpenseEntry(Guid publicId)
        {
            using var connection = _db.CreateConnection();
            var p = new DynamicParameters();
            p.Add("@PublicId", publicId);
            await connection.ExecuteAsync(
                "sp_DeleteMonthlyExpenseEntry", p,
                commandType: System.Data.CommandType.StoredProcedure
            );
        }
    }
}