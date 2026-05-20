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
            // Navbar
            services.AddScoped<INavService, NavService>();
            // Dashboard
            services.AddScoped<IDashboardService, DashboardService>();
            // Settings
            services.AddScoped<ISettingsService, SettingsService>();
            //Income
            services.AddScoped<IIncomeService, IncomeService>();

            services.AddScoped<IEmploymentService, EmploymentService>();

            //Expense
            services.AddScoped<IExpenseTypeService, ExpenseTypeService>();

            //CC
            services.AddScoped<ICreditCardService, CreditCardService>();

            // transaction
            services.AddScoped<ITransactionService, TransactionService>();
        }
    }
}