using StockPilot.Api.Configuration;
using StockPilot.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Seed database roles and promote the existing administrator.
await using (var scope = app.Services.CreateAsyncScope())
{
    var initializer = scope.ServiceProvider
        .GetRequiredService<IDbAdminInitializer>();

    await initializer.SeedAsync(
        app.Configuration,
        app.Lifetime.ApplicationStopping);
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "StockPilot API v1"));
}
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
// REMEMBER TO ADD EXPENSES

//To run 
//  dotnet run --project .\src\StockPilot.Api\StockPilot.Api.csproj

//Migration
//dotnet ef migrations add InitialPostgresCreate `
//  --project .\src\StockPilot.Infrastructure\StockPilot.Infrastructure.csproj `
//  --startup-project .\src\StockPilot.Api\StockPilot.Api.csproj `
//  --output-dir Persistence\Migrations

// dotnet ef database update `
//  --project .\src\StockPilot.Infrastructure\StockPilot.Infrastructure.csproj `
//  --startup-project .\src\StockPilot.Api\StockPilot.Api.csproj

