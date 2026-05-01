using Dapper;
using FinanceTracker.Data;
using FinanceTracker.Models.Employment;
using System.Text.Json;

namespace FinanceTracker.Services
{
    public interface IEmploymentService
    {
        Task<IEnumerable<EmploymentTypeItem>> GetEmploymentTypes();
        Task<IEnumerable<PayFrequencyItem>> GetPayFrequencies();
        Task<IEnumerable<EmploymentItem>> GetActiveEmployments(Guid userPublicId, int month, int year);
        Task<int> CreateEmployment(Guid userPublicId, CreateEmploymentRequest request);
        Task UpdateEmployment(UpdateEmploymentRequest request);
        Task DeleteEmployment(Guid publicId);
        Task<IEnumerable<MonthlySalaryItem>> GetMonthlySalaries(Guid userPublicId, int month, int year);
        Task<Guid> UpsertMonthlySalary(Guid userPublicId, UpsertMonthlySalaryRequest request);
        Task DeleteMonthlySalary(Guid publicId);
    }

    public class EmploymentService : IEmploymentService
    {
        private readonly DbContext _db;

        public EmploymentService(DbContext db)
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

        public async Task<IEnumerable<EmploymentTypeItem>> GetEmploymentTypes()
        {
            using var connection = _db.CreateConnection();
            return await connection.QueryAsync<EmploymentTypeItem>(
                "sp_GetEmploymentTypes",
                commandType: System.Data.CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<PayFrequencyItem>> GetPayFrequencies()
        {
            using var connection = _db.CreateConnection();
            return await connection.QueryAsync<PayFrequencyItem>(
                "sp_GetPayFrequencies",
                commandType: System.Data.CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<EmploymentItem>> GetActiveEmployments(
            Guid userPublicId, int month, int year)
        {
            using var connection = _db.CreateConnection();
            var userId = await GetUserId(userPublicId);
            var p = new DynamicParameters();
            p.Add("@UserId", userId);
            p.Add("@Month", month);
            p.Add("@Year", year);
            return await connection.QueryAsync<EmploymentItem>(
                "sp_GetActiveEmployments", p,
                commandType: System.Data.CommandType.StoredProcedure
            );
        }

        public async Task<int> CreateEmployment(
            Guid userPublicId, CreateEmploymentRequest request)
        {
            using var connection = _db.CreateConnection();
            var userId = await GetUserId(userPublicId);

            var payload = new
            {
                UserId = userId,
                request.CompanyName,
                request.JobRole,
                request.EmploymentTypeId,
                request.PayFrequencyId,
                StartDate = request.StartDate.ToString("yyyy-MM-dd"),
                EndDate = request.EndDate?.ToString("yyyy-MM-dd")
            };

            var p = new DynamicParameters();
            p.Add("@JsonData", JsonSerializer.Serialize(payload));

            var result = await connection.QueryFirstOrDefaultAsync<int>(
                "sp_CreateEmployment", p,
                commandType: System.Data.CommandType.StoredProcedure
            );

            if (result == 0) throw new Exception("Failed to create employment.");
            return result;
        }

        public async Task UpdateEmployment(UpdateEmploymentRequest request)
        {
            using var connection = _db.CreateConnection();

            var payload = new
            {
                PublicId = request.PublicId.ToString(),
                request.CompanyName,
                request.JobRole,
                request.EmploymentTypeId,
                request.PayFrequencyId,
                StartDate = request.StartDate.ToString("yyyy-MM-dd"),
                EndDate = request.EndDate?.ToString("yyyy-MM-dd")
            };

            var p = new DynamicParameters();
            p.Add("@JsonData", JsonSerializer.Serialize(payload));

            await connection.ExecuteAsync(
                "sp_UpdateEmployment", p,
                commandType: System.Data.CommandType.StoredProcedure
            );
        }

        public async Task DeleteEmployment(Guid publicId)
        {
            using var connection = _db.CreateConnection();
            var p = new DynamicParameters();
            p.Add("@PublicId", publicId);
            await connection.ExecuteAsync(
                "sp_DeleteEmployment", p,
                commandType: System.Data.CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<MonthlySalaryItem>> GetMonthlySalaries(
            Guid userPublicId, int month, int year)
        {
            using var connection = _db.CreateConnection();
            var userId = await GetUserId(userPublicId);
            var p = new DynamicParameters();
            p.Add("@UserId", userId);
            p.Add("@Month", month);
            p.Add("@Year", year);
            return await connection.QueryAsync<MonthlySalaryItem>(
                "sp_GetMonthlySalaries", p,
                commandType: System.Data.CommandType.StoredProcedure
            );
        }

        public async Task<Guid> UpsertMonthlySalary(
            Guid userPublicId, UpsertMonthlySalaryRequest request)
        {
            using var connection = _db.CreateConnection();
            var userId = await GetUserId(userPublicId);

            var payload = new
            {
                UserId = userId,
                request.EmploymentId,
                request.Month,
                request.Year,
                request.NetPay,
                PublicId = request.PublicId?.ToString()
            };

            var p = new DynamicParameters();
            p.Add("@JsonData", JsonSerializer.Serialize(payload));

            var result = await connection.QueryFirstOrDefaultAsync<Guid>(
                "sp_UpsertMonthlySalary", p,
                commandType: System.Data.CommandType.StoredProcedure
            );

            if (result == Guid.Empty)
                throw new Exception("Failed to save salary.");

            return result;
        }

        public async Task DeleteMonthlySalary(Guid publicId)
        {
            using var connection = _db.CreateConnection();
            var p = new DynamicParameters();
            p.Add("@PublicId", publicId);
            await connection.ExecuteAsync(
                "sp_DeleteMonthlySalary", p,
                commandType: System.Data.CommandType.StoredProcedure
            );
        }
    }
}