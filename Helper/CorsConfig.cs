namespace FinanceTracker.Helper
{
    public static class CorsConfig
    {
        public static void Configure(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy
                        .WithOrigins(
                            "http://localhost:5173",
                            "http://localhost:3000",
                            "http://10.0.0.27:5173"
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
        }
    }
}