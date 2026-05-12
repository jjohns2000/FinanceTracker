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
                            "http://10.0.0.27:5173", //change here if the device IP changes
                            "https://salmon-river-048dde10f.7.azurestaticapps.net", //Azure static apps
                            "https://financetracker.itsjoeljohnson.com/"
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
        }
    }
}