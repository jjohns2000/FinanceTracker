using Dapper;
using FinanceTracker.Data;
using FinanceTracker.Models.Nav;

namespace FinanceTracker.Services
{
    public interface INavService
    {
        Task<IEnumerable<NavItem>> GetNavItems();
    }

    public class NavService : INavService
    {
        private readonly DbContext _db;

        public NavService(DbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<NavItem>> GetNavItems()
        {
            using var connection = _db.CreateConnection();

            return await connection.QueryAsync<NavItem>(
                "sp_GetNavItems",
                commandType: System.Data.CommandType.StoredProcedure
            );
        }
    }
}