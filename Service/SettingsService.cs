using Dapper;
using FinanceTracker.Data;
using FinanceTracker.Models.Settings;
using System.Text.Json;

namespace FinanceTracker.Services
{
    public interface ISettingsService
    {
        Task<IEnumerable<BankItem>> GetBanks();
        Task<IEnumerable<AccountTypeItem>> GetAccountTypes();
        Task<IEnumerable<UserAccountItem>> GetUserAccounts(Guid publicId);
        Task<int> CreateUserAccount(Guid publicId, CreateUserAccountRequest request);
        Task UpdateUserAccount(UpdateUserAccountRequest request);
        Task DeleteUserAccount(Guid publicId);
    }

    public class SettingsService : ISettingsService
    {
        private readonly DbContext _db;

        public SettingsService(DbContext db)
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

        public async Task<IEnumerable<BankItem>> GetBanks()
        {
            using var connection = _db.CreateConnection();
            return await connection.QueryAsync<BankItem>(
                "sp_GetBanks",
                commandType: System.Data.CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<AccountTypeItem>> GetAccountTypes()
        {
            using var connection = _db.CreateConnection();
            return await connection.QueryAsync<AccountTypeItem>(
                "sp_GetAccountTypes",
                commandType: System.Data.CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<UserAccountItem>> GetUserAccounts(Guid publicId)
        {
            using var connection = _db.CreateConnection();
            var userId = await GetUserId(publicId);
            var p = new DynamicParameters();
            p.Add("@UserId", userId);
            return await connection.QueryAsync<UserAccountItem>(
                "sp_GetUserAccounts", p,
                commandType: System.Data.CommandType.StoredProcedure
            );
        }

        public async Task<int> CreateUserAccount(Guid publicId, CreateUserAccountRequest request)
        {
            using var connection = _db.CreateConnection();
            var userId = await GetUserId(publicId);

            var payload = new
            {
                UserId = userId,
                request.BankId,
                request.AccountTypeId,
                request.OpeningBalance
            };

            var p = new DynamicParameters();
            p.Add("@JsonData", JsonSerializer.Serialize(payload));

            var result = await connection.QueryFirstOrDefaultAsync<int>(
                "sp_CreateUserAccount", p,
                commandType: System.Data.CommandType.StoredProcedure
            );

            if (result == 0) throw new Exception("Failed to create account.");
            return result;
        }

        public async Task UpdateUserAccount(UpdateUserAccountRequest request)
        {
            using var connection = _db.CreateConnection();
            var p = new DynamicParameters();
            p.Add("@JsonData", JsonSerializer.Serialize(request));
            await connection.ExecuteAsync(
                "sp_UpdateUserAccount", p,
                commandType: System.Data.CommandType.StoredProcedure
            );
        }

        public async Task DeleteUserAccount(Guid publicId)
        {
            using var connection = _db.CreateConnection();
            var p = new DynamicParameters();
            p.Add("@PublicId", publicId);
            await connection.ExecuteAsync(
                "sp_DeleteUserAccount", p,
                commandType: System.Data.CommandType.StoredProcedure
            );
        }
    }
}