using JwtNugetClassLibrary.Manager;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JwtNugetClassLibrary
{
    public static class JwtBearerAuthenticationExtensions
    {
        public static void AddCustomJwtBearerAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("JWT");

            var validAudience = jwtSettings.GetValue<string>("ValidAudience");
            var validIssuer = jwtSettings.GetValue<string>("ValidIssuer");
            var secret = jwtSettings.GetValue<string>("Secret");
            var tokenValidityInMinutes = jwtSettings.GetValue<int>("TokenValidityInMinutes");
            var refreshTokenValidityInDays = jwtSettings.GetValue<int>("RefreshTokenValidityInDays");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.Zero,

                        ValidAudience = validAudience,
                        ValidIssuer = validIssuer,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret!))
                    };
                });

            services.AddMemoryCache();

            services.AddSingleton<JwtManager, JwtManager>();
            services.AddSingleton<CachingManager, CachingManager>();
            services.AddSingleton<DbManager, DbManager>();
        }
    }
}
