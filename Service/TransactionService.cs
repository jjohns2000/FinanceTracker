using Dapper;
using FinanceTracker.Data;
using FinanceTracker.Models.Transaction;
using System.Text.Json;

namespace FinanceTracker.Services
{
    public interface ITransactionService
    {
        Task<IEnumerable<PaymentMethodItem>> GetPaymentMethods();
        Task<IEnumerable<TransactionItem>> GetTransactions(Guid userPublicId, int month, int year);
        Task<int> CreateTransaction(Guid userPublicId, CreateTransactionRequest request);
        Task UpdateTransaction(UpdateTransactionRequest request);
        Task DeleteTransaction(Guid publicId);
    }

    public class TransactionService : ITransactionService
    {
        private readonly DbContext _db;

        public TransactionService(DbContext db)
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

        public async Task<IEnumerable<PaymentMethodItem>> GetPaymentMethods()
        {
            using var connection = _db.CreateConnection();
            return await connection.QueryAsync<PaymentMethodItem>(
                "sp_GetPaymentMethods",
                commandType: System.Data.CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<TransactionItem>> GetTransactions(
            Guid userPublicId, int month, int year)
        {
            using var connection = _db.CreateConnection();
            var userId = await GetUserId(userPublicId);
            var p = new DynamicParameters();
            p.Add("@UserId", userId);
            p.Add("@Month", month);
            p.Add("@Year", year);
            return await connection.QueryAsync<TransactionItem>(
                "sp_GetTransactions", p,
                commandType: System.Data.CommandType.StoredProcedure
            );
        }

        public async Task<int> CreateTransaction(
            Guid userPublicId, CreateTransactionRequest request)
        {
            using var connection = _db.CreateConnection();
            var userId = await GetUserId(userPublicId);

            var payload = new
            {
                UserId = userId,
                request.ExpenseTypeId,
                request.AccountId,
                request.CreditCardId,
                request.PaymentMethodId,
                request.Amount,
                request.Description,
                TransactionDate = request.TransactionDate.ToString("yyyy-MM-dd")
            };

            var p = new DynamicParameters();
            p.Add("@JsonData", JsonSerializer.Serialize(payload));

            var result = await connection.QueryFirstOrDefaultAsync<int>(
                "sp_CreateTransaction", p,
                commandType: System.Data.CommandType.StoredProcedure
            );

            if (result == 0) throw new Exception("Failed to create transaction.");
            return result;
        }

        public async Task UpdateTransaction(UpdateTransactionRequest request)
        {
            using var connection = _db.CreateConnection();

            var payload = new
            {
                PublicId = request.PublicId.ToString(),
                request.ExpenseTypeId,
                request.AccountId,
                request.CreditCardId,
                request.PaymentMethodId,
                request.Amount,
                request.Description,
                TransactionDate = request.TransactionDate.ToString("yyyy-MM-dd")
            };

            var p = new DynamicParameters();
            p.Add("@JsonData", JsonSerializer.Serialize(payload));

            await connection.ExecuteAsync(
                "sp_UpdateTransaction", p,
                commandType: System.Data.CommandType.StoredProcedure
            );
        }

        public async Task DeleteTransaction(Guid publicId)
        {
            using var connection = _db.CreateConnection();
            var p = new DynamicParameters();
            p.Add("@PublicId", publicId);
            await connection.ExecuteAsync(
                "sp_DeleteTransaction", p,
                commandType: System.Data.CommandType.StoredProcedure
            );
        }
    }
}