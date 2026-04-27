using Dapper;
using FinanceTracker.Data;
using FinanceTracker.Models.Income;
using System.Text.Json;

namespace FinanceTracker.Services
{
    public interface IIncomeService
    {
        Task<IEnumerable<MonthlyAccountSummaryItem>> GetMonthlyAccountSummary(Guid userPublicId, int month, int year);
        Task<MonthlyAggregateItem> GetMonthlyAggregate(Guid userPublicId, int month, int year);
        Task<Guid> UpsertDeposit(Guid userPublicId, UpsertDepositRequest request);
        Task<Guid> UpsertWithdrawal(Guid userPublicId, UpsertWithdrawalRequest request);
        Task<IEnumerable<AccountRunningTotal>> GetAccountRunningTotals(Guid userPublicId);
    }

    public class IncomeService : IIncomeService
    {
        private readonly DbContext _db;

        public IncomeService(DbContext db)
        {
            _db = db;
        }

        private async Task<(int UserId, int AccountId)> ResolveIds(Guid userPublicId, Guid accountPublicId)
        {
            using var connection = _db.CreateConnection();

            var userParams = new DynamicParameters();
            userParams.Add("@PublicId", userPublicId);

            var user = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "sp_GetUserByPublicId",
                userParams,
                commandType: System.Data.CommandType.StoredProcedure
            );

            if (user == null)
                throw new Exception("User not found.");

            var accountParams = new DynamicParameters();
            accountParams.Add("@PublicId", accountPublicId);

            var account = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "sp_GetUserAccountByPublicId",
                accountParams,
                commandType: System.Data.CommandType.StoredProcedure
            );

            if (account == null)
                throw new Exception("Account not found.");

            return ((int)user.Id, (int)account.Id);
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

        public async Task<IEnumerable<MonthlyAccountSummaryItem>> GetMonthlyAccountSummary(
            Guid userPublicId, int month, int year)
        {
            using var connection = _db.CreateConnection();
            var userId = await GetUserId(userPublicId);

            var p = new DynamicParameters();
            p.Add("@UserId", userId);
            p.Add("@Month", month);
            p.Add("@Year", year);

            return await connection.QueryAsync<MonthlyAccountSummaryItem>(
                "sp_GetMonthlyAccountSummary", p,
                commandType: System.Data.CommandType.StoredProcedure
            );
        }

        public async Task<MonthlyAggregateItem> GetMonthlyAggregate(
            Guid userPublicId, int month, int year)
        {
            using var connection = _db.CreateConnection();
            var userId = await GetUserId(userPublicId);

            var p = new DynamicParameters();
            p.Add("@UserId", userId);
            p.Add("@Month", month);
            p.Add("@Year", year);

            var result = await connection.QueryFirstOrDefaultAsync<MonthlyAggregateItem>(
                "sp_GetMonthlyAggregate", p,
                commandType: System.Data.CommandType.StoredProcedure
            );

            return result ?? new MonthlyAggregateItem();
        }

        
        public async Task<Guid> UpsertDeposit(Guid userPublicId, UpsertDepositRequest request)
        {
            using var connection = _db.CreateConnection();
            var (userId, accountId) = await ResolveIds(userPublicId, request.AccountPublicId);

            var payload = new
            {
                UserId = userId,
                AccountId = accountId,
                request.Month,
                request.Year,
                request.Deposit
            };

            var p = new DynamicParameters();
            p.Add("@JsonData", JsonSerializer.Serialize(payload));

            var result = await connection.QueryFirstOrDefaultAsync<Guid>(
                "sp_UpsertMonthlyDeposit", p,
                commandType: System.Data.CommandType.StoredProcedure
            );

            if (result == Guid.Empty)
                throw new Exception("Failed to save deposit.");

            return result;
        }

        public async Task<Guid> UpsertWithdrawal(Guid userPublicId, UpsertWithdrawalRequest request)
        {
            using var connection = _db.CreateConnection();
            var (userId, accountId) = await ResolveIds(userPublicId, request.AccountPublicId);

            var payload = new
            {
                UserId = userId,
                AccountId = accountId,
                request.Month,
                request.Year,
                request.Withdrawal
            };

            var p = new DynamicParameters();
            p.Add("@JsonData", JsonSerializer.Serialize(payload));

            var result = await connection.QueryFirstOrDefaultAsync<Guid>(
                "sp_UpsertMonthlyWithdrawal", p,
                commandType: System.Data.CommandType.StoredProcedure
            );

            if (result == Guid.Empty)
                throw new Exception("Failed to save withdrawal.");

            return result;
        }
        public async Task<IEnumerable<AccountRunningTotal>> GetAccountRunningTotals(Guid userPublicId)
        {
            using var connection = _db.CreateConnection();
            var userId = await GetUserId(userPublicId);

            var p = new DynamicParameters();
            p.Add("@UserId", userId);

            return await connection.QueryAsync<AccountRunningTotal>(
                "sp_GetAccountRunningTotal", p,
                commandType: System.Data.CommandType.StoredProcedure
            );
        }
    }
}