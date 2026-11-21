using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace StockPilot.Api.Configuration
{
    public static class AuthConfig
    {
        public static IServiceCollection AddAuthenticationConfig(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            var secret = configuration["AppSettings:SecretKey"];
            if (string.IsNullOrWhiteSpace(secret))
                throw new InvalidOperationException("Jwt Secret key is missing");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

            var issuer =
                configuration["AppSettings:Issuer"]
                ?? throw new InvalidOperationException("Jwt Issuer is missing");

            var audience =
                configuration["AppSettings:Audience"]
                ?? throw new InvalidOperationException("Jwt Audience is missing");

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = key,

                        ValidateIssuer = true,
                        ValidIssuer = issuer,

                        ValidateAudience = true,
                        ValidAudience = audience,

                        ValidateLifetime = true,

                    };
                });
            return services;
        }
    }
}
