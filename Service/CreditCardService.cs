using Dapper;
using FinanceTracker.Data;
using FinanceTracker.Models.CreditCard;
using System.Text.Json;

namespace FinanceTracker.Services
{
    public interface ICreditCardService
    {
        Task<IEnumerable<CreditCardItem>> GetUserCreditCards(Guid userPublicId);
        Task<int> CreateCreditCard(Guid userPublicId, CreateCreditCardRequest request);
        Task UpdateCreditCard(UpdateCreditCardRequest request);
        Task DeleteCreditCard(Guid publicId);
        Task<IEnumerable<MonthlyCreditCardSummaryItem>> GetMonthlyCreditCardSummary(Guid userPublicId, int month, int year);
        Task<Guid> UpsertMonthlyCreditCardSummary(Guid userPublicId, UpsertMonthlyCreditCardRequest request);
    }

    public class CreditCardService : ICreditCardService
    {
        private readonly DbContext _db;

        public CreditCardService(DbContext db)
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

        public async Task<IEnumerable<CreditCardItem>> GetUserCreditCards(Guid userPublicId)
        {
            using var connection = _db.CreateConnection();
            var userId = await GetUserId(userPublicId);
            var p = new DynamicParameters();
            p.Add("@UserId", userId);
            return await connection.QueryAsync<CreditCardItem>(
                "sp_GetUserCreditCards", p,
                commandType: System.Data.CommandType.StoredProcedure
            );
        }

        public async Task<int> CreateCreditCard(Guid userPublicId, CreateCreditCardRequest request)
        {
            using var connection = _db.CreateConnection();
            var userId = await GetUserId(userPublicId);

            var payload = new
            {
                UserId = userId,
                request.CardName,
                request.CreditLimit,
                StartDate = request.StartDate.ToString("yyyy-MM-dd"),
                CardColor = string.IsNullOrEmpty(request.CardColor) ? "#f44336" : request.CardColor
            };

            var p = new DynamicParameters();
            p.Add("@JsonData", JsonSerializer.Serialize(payload));

            var result = await connection.QueryFirstOrDefaultAsync<int>(
                "sp_CreateCreditCard", p,
                commandType: System.Data.CommandType.StoredProcedure
            );

            if (result == 0) throw new Exception("Failed to create credit card.");
            return result;
        }

        public async Task UpdateCreditCard(UpdateCreditCardRequest request)
        {
            using var connection = _db.CreateConnection();

            var payload = new
            {
                PublicId = request.PublicId.ToString(),
                request.CardName,
                request.CreditLimit,
                StartDate = request.StartDate.ToString("yyyy-MM-dd"),
                CardColor = string.IsNullOrEmpty(request.CardColor) ? "#f44336" : request.CardColor
            };

            var p = new DynamicParameters();
            p.Add("@JsonData", JsonSerializer.Serialize(payload));

            await connection.ExecuteAsync(
                "sp_UpdateCreditCard", p,
                commandType: System.Data.CommandType.StoredProcedure
            );
        }

        public async Task DeleteCreditCard(Guid publicId)
        {
            using var connection = _db.CreateConnection();
            var p = new DynamicParameters();
            p.Add("@PublicId", publicId);
            await connection.ExecuteAsync(
                "sp_DeleteCreditCard", p,
                commandType: System.Data.CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<MonthlyCreditCardSummaryItem>> GetMonthlyCreditCardSummary(
            Guid userPublicId, int month, int year)
        {
            using var connection = _db.CreateConnection();
            var userId = await GetUserId(userPublicId);
            var p = new DynamicParameters();
            p.Add("@UserId", userId);
            p.Add("@Month", month);
            p.Add("@Year", year);
            return await connection.QueryAsync<MonthlyCreditCardSummaryItem>(
                "sp_GetMonthlyCreditCardSummary", p,
                commandType: System.Data.CommandType.StoredProcedure
            );
        }

        public async Task<Guid> UpsertMonthlyCreditCardSummary(
            Guid userPublicId, UpsertMonthlyCreditCardRequest request)
        {
            using var connection = _db.CreateConnection();
            var userId = await GetUserId(userPublicId);

            var payload = new
            {
                UserId = userId,
                request.CreditCardId,
                request.Month,
                request.Year,
                request.BillAmount,
                request.AmountPaid
            };

            var p = new DynamicParameters();
            p.Add("@JsonData", JsonSerializer.Serialize(payload));

            var result = await connection.QueryFirstOrDefaultAsync<Guid>(
                "sp_UpsertMonthlyCreditCardSummary", p,
                commandType: System.Data.CommandType.StoredProcedure
            );

            if (result == Guid.Empty)
                throw new Exception("Failed to save credit card summary.");

            return result;
        }
    }
}