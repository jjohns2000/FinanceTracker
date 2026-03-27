using Microsoft.AspNetCore.Identity;

namespace FinanceTracker.Helper
{
    public class PasswordHelper
    {
        private readonly IPasswordHasher<object> _hasher;

        public PasswordHelper(IPasswordHasher<object> hasher)
        {
            _hasher = hasher;
        }

        public string HashPassword(string password)
        {
            return _hasher.HashPassword(null!, password);
        }

        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            var result = _hasher.VerifyHashedPassword(null!, hashedPassword, providedPassword);
            return result == PasswordVerificationResult.Success;
        }
    }
}