using Dapper;
using FinanceTracker.Data;
using FinanceTracker.Models.Transaction;
using System.Text.Json;

namespace FinanceTracker.Services
{
    public interface ITransactionService
    {
        Task<IEnumerable<TransactionItem>> GetTransactions(Guid userPublicId, int month, int year);
        Task<Guid> CreateTransaction(Guid userPublicId, CreateTransactionRequest request);
        Task UpdateTransaction(Guid userPublicId, UpdateTransactionRequest request);
        Task DeleteTransaction(Guid userPublicId, Guid publicId);
        Task<Guid> CreateTransfer(Guid userPublicId, CreateTransferRequest request);
        Task DeleteTransfer(Guid userPublicId, Guid publicId);
        Task<IEnumerable<dynamic>> GetPaymentMethods();

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
                commandType: System.Data.CommandType.StoredProcedure);
            if (user == null) throw new Exception("User not found.");
            return (int)user.Id;
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
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<Guid> CreateTransaction(
            Guid userPublicId, CreateTransactionRequest request)
        {
            using var connection = _db.CreateConnection();
            var userId = await GetUserId(userPublicId);
            var p = new DynamicParameters();
            p.Add("@JsonData", JsonSerializer.Serialize(new
            {
                UserId = userId,
                request.ExpenseTypeId,
                request.PaymentMethodId,
                request.Amount,
                request.TransactionDate,
                request.Description,
                request.AccountId,
                request.CreditCardId
            }));
            var result = await connection.QueryFirstOrDefaultAsync<Guid>(
                "sp_CreateTransaction", p,
                commandType: System.Data.CommandType.StoredProcedure);
            if (result == Guid.Empty)
                throw new Exception("Failed to create transaction.");
            return result;
        }

        public async Task UpdateTransaction(
            Guid userPublicId, UpdateTransactionRequest request)
        {
            using var connection = _db.CreateConnection();
            var userId = await GetUserId(userPublicId);
            var p = new DynamicParameters();
            p.Add("@JsonData", JsonSerializer.Serialize(new
            {
                UserId = userId,
                request.PublicId,
                request.ExpenseTypeId,
                request.PaymentMethodId,
                request.Amount,
                request.TransactionDate,
                request.Description,
                request.AccountId,
                request.CreditCardId
            }));
            await connection.ExecuteAsync(
                "sp_UpdateTransaction", p,
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task DeleteTransaction(Guid userPublicId, Guid publicId)
        {
            using var connection = _db.CreateConnection();
            var userId = await GetUserId(userPublicId);
            var p = new DynamicParameters();
            p.Add("@PublicId", publicId);
            p.Add("@UserId", userId);
            await connection.ExecuteAsync(
                "sp_DeleteTransaction", p,
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<Guid> CreateTransfer(
            Guid userPublicId, CreateTransferRequest request)
        {
            using var connection = _db.CreateConnection();
            var userId = await GetUserId(userPublicId);

            var fromAccountId = await connection.QueryFirstOrDefaultAsync<int>(
                "SELECT Id FROM UserAccounts WHERE PublicId = @PublicId AND UserId = @UserId",
                new { PublicId = request.FromAccountPublicId, UserId = userId });

            var toAccountId = await connection.QueryFirstOrDefaultAsync<int>(
                "SELECT Id FROM UserAccounts WHERE PublicId = @PublicId AND UserId = @UserId",
                new { PublicId = request.ToAccountPublicId, UserId = userId });

            if (fromAccountId == 0)
                throw new Exception("Source account not found.");
            if (toAccountId == 0)
                throw new Exception("Destination account not found.");

            var p = new DynamicParameters();
            p.Add("@JsonData", JsonSerializer.Serialize(new
            {
                UserId = userId,
                FromAccountId = fromAccountId,
                ToAccountId = toAccountId,
                Amount = request.Amount,
                Date = request.TransactionDate,
                Description = request.Description
            }));

            var result = await connection.QueryFirstOrDefaultAsync<Guid>(
                "sp_CreateTransfer", p,
                commandType: System.Data.CommandType.StoredProcedure);

            if (result == Guid.Empty)
                throw new Exception("Failed to create transfer.");

            return result;
        }

        public async Task DeleteTransfer(Guid userPublicId, Guid publicId)
        {
            using var connection = _db.CreateConnection();
            var userId = await GetUserId(userPublicId);
            var p = new DynamicParameters();
            p.Add("@PublicId", publicId);
            p.Add("@UserId", userId);
            await connection.ExecuteAsync(
                "sp_DeleteTransfer", p,
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<dynamic>> GetPaymentMethods()
        {
            using var connection = _db.CreateConnection();
            return await connection.QueryAsync(
                "SELECT Id AS id, MethodName AS methodName FROM PaymentMethods ORDER BY Id");
        }
    }
}