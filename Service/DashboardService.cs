using Dapper;
using FinanceTracker.Data;
using FinanceTracker.Models.Dashboard;

namespace FinanceTracker.Services
{
    public interface IDashboardService
    {
        Task<KpiData> GetKpiData(Guid publicId, int month, int year);
        Task<IEnumerable<MonthlyTrendItem>> GetMonthlyTrend(Guid publicId, int month, int year);
        Task<IEnumerable<PieDataItem>> GetIncomePieData(Guid publicId, int month, int year);
        Task<IEnumerable<PieDataItem>> GetExpensePieData(Guid publicId, int month, int year);
        Task<IEnumerable<SalaryTrendItem>> GetSalaryTrend(Guid publicId, int month, int year);
        Task<FinancialSummaryResponse> GetFinancialSummary(Guid publicId, int month, int year);
    }

    public class DashboardService : IDashboardService
    {
        private readonly DbContext _db;

        public DashboardService(DbContext db)
        {
            _db = db;
        }

        private async Task<int> GetUserId(Guid publicId)
        {
            using var connection = _db.CreateConnection();
            var p = new DynamicParameters();
            p.Add("@PublicId", publicId);
            var user = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "sp_GetUserByPublicId", p,
                commandType: System.Data.CommandType.StoredProcedure
            );
            if (user == null) throw new Exception("User not found.");
            return (int)user.Id;
        }

        public async Task<KpiData> GetKpiData(Guid publicId, int month, int year)
        {
            using var connection = _db.CreateConnection();
            var userId = await GetUserId(publicId);

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

        public async Task<IEnumerable<MonthlyTrendItem>> GetMonthlyTrend(
            Guid publicId, int month, int year)
        {
            using var connection = _db.CreateConnection();
            var userId = await GetUserId(publicId);

            // Calculate from month (12 months back)
            var fromDate = new DateTime(year, month, 1).AddMonths(-11);

            var p = new DynamicParameters();
            p.Add("@UserId", userId);
            p.Add("@FromMonth", fromDate.Month);
            p.Add("@FromYear", fromDate.Year);
            p.Add("@ToMonth", month);
            p.Add("@ToYear", year);

            return await connection.QueryAsync<MonthlyTrendItem>(
                "sp_GetMonthlyTrend", p,
                commandType: System.Data.CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<PieDataItem>> GetIncomePieData(
            Guid publicId, int month, int year)
        {
            using var connection = _db.CreateConnection();
            var userId = await GetUserId(publicId);

            var p = new DynamicParameters();
            p.Add("@UserId", userId);
            p.Add("@Month", month);
            p.Add("@Year", year);

            return await connection.QueryAsync<PieDataItem>(
                "sp_GetIncomePieData", p,
                commandType: System.Data.CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<PieDataItem>> GetExpensePieData(
            Guid publicId, int month, int year)
        {
            using var connection = _db.CreateConnection();
            var userId = await GetUserId(publicId);

            var p = new DynamicParameters();
            p.Add("@UserId", userId);
            p.Add("@Month", month);
            p.Add("@Year", year);

            return await connection.QueryAsync<PieDataItem>(
                "sp_GetExpensePieData", p,
                commandType: System.Data.CommandType.StoredProcedure
            );
        }
        public async Task<IEnumerable<SalaryTrendItem>> GetSalaryTrend(Guid publicId, int month, int year)
            {
                using var connection = _db.CreateConnection();
                var userId = await GetUserId(publicId);

                var fromDate = new DateTime(year, month, 1).AddMonths(-11);

                var p = new DynamicParameters();
                p.Add("@UserId", userId);
                p.Add("@FromMonth", fromDate.Month);
                p.Add("@FromYear", fromDate.Year);
                p.Add("@ToMonth", month);
                p.Add("@ToYear", year);

                return await connection.QueryAsync<SalaryTrendItem>(
                    "sp_GetSalaryTrend", p,
                    commandType: System.Data.CommandType.StoredProcedure
                );
            }
        public async Task<FinancialSummaryResponse> GetFinancialSummary(
    Guid publicId, int month, int year)
        {
            using var connection = _db.CreateConnection();
            var userId = await GetUserId(publicId);

            var p = new DynamicParameters();
            p.Add("@UserId", userId);
            p.Add("@Month", month);
            p.Add("@Year", year);

            var result = await connection.QueryFirstOrDefaultAsync<string>(
                "sp_GetFinancialSummary", p,
                commandType: System.Data.CommandType.StoredProcedure
            );

            return new FinancialSummaryResponse
            {
                Summary = result ?? string.Empty
            };
        }
    }
}