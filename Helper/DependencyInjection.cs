using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using FinanceTracker.Services;
using FinanceTracker.Helper;
using FinanceTracker.Data;

namespace FinanceTracker.Helper
{
    public static class DependencyInjection
    {
        public static void RegisterServices(IServiceCollection services)
        {
            // Helpers
            services.AddSingleton<JwtHelper>();
            services.AddSingleton<PasswordHelper>();
            services.AddSingleton<IPasswordHasher<object>, PasswordHasher<object>>();

            // DB Context
            services.AddSingleton<DbContext>();
            // Auth Services
            services.AddScoped<IAuthService, AuthService>();
        }
    }
}