using Dapper;
using FinanceTracker.Data;
using FinanceTracker.Models.Dashboard;

namespace FinanceTracker.Services
{
    public interface IDashboardService
    {
        Task<KpiData> GetKpiData(Guid publicId, int month, int year);
    }

    public class DashboardService : IDashboardService
    {
        private readonly DbContext _db;

        public DashboardService(DbContext db)
        {
            _db = db;
        }

        public async Task<KpiData> GetKpiData(Guid publicId, int month, int year)
        {
            using var connection = _db.CreateConnection();

            var idParams = new DynamicParameters();
            idParams.Add("@PublicId", publicId);

            var user = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "sp_GetUserByPublicId",
                idParams,
                commandType: System.Data.CommandType.StoredProcedure
            );

            if (user == null)
                throw new Exception("User not found.");

            int userId = user.Id;

            var p = new DynamicParameters();
            p.Add("@UserId", userId);
            p.Add("@Month", month);
            p.Add("@Year", year);

            var income = await connection.QueryFirstOrDefaultAsync<KpiItem>(
                "sp_GetTotalIncome", p,
                commandType: System.Data.CommandType.StoredProcedure
            );

            var expenses = await connection.QueryFirstOrDefaultAsync<KpiItem>(
                "sp_GetTotalExpenses", p,
                commandType: System.Data.CommandType.StoredProcedure
            );

            var balance = await connection.QueryFirstOrDefaultAsync<KpiItem>(
                "sp_GetNetBalance", p,
                commandType: System.Data.CommandType.StoredProcedure
            );

            var budget = await connection.QueryFirstOrDefaultAsync<KpiItem>(
                "sp_GetBudgetRemaining", p,
                commandType: System.Data.CommandType.StoredProcedure
            );

            return new KpiData
            {
                TotalIncome = income ?? new KpiItem { Label = "Total Income", Value = 0 },
                TotalExpenses = expenses ?? new KpiItem { Label = "Total Expenses", Value = 0 },
                NetBalance = balance ?? new KpiItem { Label = "Net Balance", Value = 0 },
                BudgetRemaining = budget ?? new KpiItem { Label = "Budget Remaining", Value = 0 }
            };
        }
    }
}