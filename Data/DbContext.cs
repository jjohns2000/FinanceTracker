using Microsoft.Data.SqlClient;
using System.Data;

namespace FinanceTracker.Data
{
    public class DbContext
    {
        private readonly string _connectionString;

        public DbContext(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string not found.");
        }

        public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}