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
        Task<RegisterResponse> Register(RegisterRequest request);
        Task<AuthResponse> Login(LoginRequest request);
        Task<UserEntity> GetMe(Guid publicId);
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

        public async Task<RegisterResponse> Register(RegisterRequest request)
        {
            using var connection = _db.CreateConnection();

            // Check if email already exists
            var checkParams = new DynamicParameters();
            checkParams.Add("@Email", request.Email);

            var emailExists = await connection.QueryFirstOrDefaultAsync<int>(
                "sp_CheckEmailExists",
                checkParams,
                commandType: System.Data.CommandType.StoredProcedure
            );

            if (emailExists == 1)
                throw new Exception("Email is already registered.");

            // Hash password
            var passwordHash = _passwordHelper.HashPassword(request.Password);

            // Build JSON payload
            var payload = new
            {
                request.FirstName,
                request.LastName,
                request.Email,
                PasswordHash = passwordHash
            };

            var registerParams = new DynamicParameters();
            registerParams.Add("@JsonData", JsonSerializer.Serialize(payload));

            // Returns the new Id
            var result = await connection.QueryFirstOrDefaultAsync<RegisterResponse>(
            "sp_RegisterUser",
            registerParams,
            commandType: System.Data.CommandType.StoredProcedure
            );

            if (result == null)
                throw new Exception("Registration failed.");

            return result;
        }

        // ─── Login ──────────────────────────────────────────

        public async Task<AuthResponse> Login(LoginRequest request)
        {
            using var connection = _db.CreateConnection();

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

            var token = _jwtHelper.GenerateToken(user.PublicId, user.Email);

            return new AuthResponse
            {
                Token = token,
                PublicId = user.PublicId,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            };
        }

        // ─── Get Me ─────────────────────────────────────────

        public async Task<UserEntity> GetMe(Guid publicId)
        {
            using var connection = _db.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@PublicId", publicId);

            var user = await connection.QueryFirstOrDefaultAsync<UserEntity>(
                "sp_GetUserByPublicId",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure
            );

            if (user == null)
                throw new Exception("User not found.");

            return user;
        }
    }
}