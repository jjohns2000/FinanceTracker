using FinanceTracker.Helper;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

DependencyInjection.RegisterServices(builder.Services);
JwtConfig.Configure(builder.Services, builder.Configuration);
CorsConfig.Configure(builder.Services);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<FinanceTracker.Middleware.ExceptionHandler>();
app.UseCors("AllowFrontend");
app.UseAuthentication();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
