using Dapper;
using FinanceTracker.Data;
using FinanceTracker.Helper;
using FinanceTracker.Models.Auth;
using System.Text.Json;

namespace FinanceTracker.Services
{
    // ─── Interface ──────────────────────────────────────────

    public interface IAuthService
    {
        Task<AuthResponse> Register(RegisterRequest request);
        Task<AuthResponse> Login(LoginRequest request);
        Task<UserEntity> GetMe(int userId);
    }

    // ─── Implementation ─────────────────────────────────────

    public class AuthService : IAuthService
    {
        private readonly DbContext _db;
        private readonly JwtHelper _jwtHelper;
        private readonly PasswordHelper _passwordHelper;

        public AuthService(DbContext db, JwtHelper jwtHelper, PasswordHelper passwordHelper)
        {
            _db = db;
            _jwtHelper = jwtHelper;
            _passwordHelper = passwordHelper;
        }

        // ─── Register ───────────────────────────────────────

        public async Task<AuthResponse> Register(RegisterRequest request)
        {
            using var connection = _db.CreateConnection();

            // Check if email already exists — single string parameter
            var checkParams = new DynamicParameters();
            checkParams.Add("@Email", request.Email);

            var existingUser = await connection.QueryFirstOrDefaultAsync<UserEntity>(
                "sp_GetUserByEmail",
                checkParams,
                commandType: System.Data.CommandType.StoredProcedure
            );

            if (existingUser != null)
                throw new Exception("Email is already registered.");

            // Hash password before serializing
            var passwordHash = _passwordHelper.HashPassword(request.Password);

            // Build JSON payload — JSON parameter for multiple values
            var payload = new
            {
                request.FirstName,
                request.LastName,
                request.Email,
                PasswordHash = passwordHash
            };

            var registerParams = new DynamicParameters();
            registerParams.Add("@JsonData", JsonSerializer.Serialize(payload));

            var newUser = await connection.QueryFirstOrDefaultAsync<UserEntity>(
                "sp_RegisterUser",
                registerParams,
                commandType: System.Data.CommandType.StoredProcedure
            );

            if (newUser == null)
                throw new Exception("Registration failed.");

            var token = _jwtHelper.GenerateToken(newUser.Id, newUser.Email);

            return new AuthResponse
            {
                Token = token,
                Email = newUser.Email,
                FirstName = newUser.FirstName,
                LastName = newUser.LastName
            };
        }

        // ─── Login ──────────────────────────────────────────

        public async Task<AuthResponse> Login(LoginRequest request)
        {
            using var connection = _db.CreateConnection();

            // Single string parameter
            var checkParams = new DynamicParameters();
            checkParams.Add("@Email", request.Email);

            var user = await connection.QueryFirstOrDefaultAsync<UserEntity>(
                "sp_GetUserByEmail",
                checkParams,
                commandType: System.Data.CommandType.StoredProcedure
            );

            if (user == null)
                throw new Exception("Invalid email or password.");

            var isValid = _passwordHelper.VerifyPassword(user.PasswordHash, request.Password);

            if (!isValid)
                throw new Exception("Invalid email or password.");

            if (!user.IsActive)
                throw new Exception("Account is deactivated.");

            var token = _jwtHelper.GenerateToken(user.Id, user.Email);

            return new AuthResponse
            {
                Token = token,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            };
        }

        // ─── Get Me ─────────────────────────────────────────

        public async Task<UserEntity> GetMe(int userId)
        {
            using var connection = _db.CreateConnection();

            // Single int parameter
            var parameters = new DynamicParameters();
            parameters.Add("@Id", userId);

            var user = await connection.QueryFirstOrDefaultAsync<UserEntity>(
                "sp_GetUserById",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure
            );

            if (user == null)
                throw new Exception("User not found.");

            return user;
        }
    }
}