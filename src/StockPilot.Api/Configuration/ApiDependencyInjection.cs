using System.Text.Json.Serialization;
using StockPilot.Api.Middleware;
using StockPilot.Infrastructure.DependencyInjection;

namespace StockPilot.Api.Configuration;

public static class ApiDependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers().AddJsonOptions(options =>
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddAuthenticationConfig(configuration);
        services.AddSwaggerConfig();
        services.AddInfrastructure(configuration);
        return services;
    }
}
