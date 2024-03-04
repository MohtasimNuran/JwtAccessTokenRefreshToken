using JwtNugetClassLibrary.Manager;
using JwtNugetClassLibrary.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JwtNugetClassLibrary.Middlewares
{
    public class CustomJwtValidationManager
    {
        private readonly RequestDelegate _next;
        private readonly CachingManager _cachingManager;
        private readonly JwtManager _jwtManager;
        private readonly IConfiguration _configuration;

        public CustomJwtValidationManager(RequestDelegate next, CachingManager cachingManager, JwtManager jwtManager, IConfiguration configuration)
        {
            _next = next;
            _cachingManager = cachingManager;
            _jwtManager = jwtManager;
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext context)
        {
            var authorizationHeader = context.Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                context.Request.EnableBuffering();

                await _next(context);

                context.Request.Body.Position = 0;

                if (context.Response.StatusCode == 200)
                {
                    switch (context.Request.Path.ToString().ToLower())
                    {
                        case var path when path.Contains(_configuration["JWT:RegisterRoute"]!.ToLower()):
                            // Handle register logic here
                            await HandleRegister(context);
                            break;

                        case var path when path.Contains(_configuration["JWT:LoginRoute"]!.ToLower()):
                            // Handle login logic here
                            await HandleLogin(context);
                            break;
                    }
                }

                return;
            }

            var jwtToken = authorizationHeader.Substring("Bearer ".Length);

            var userName = _jwtManager.GetUserNameFromToken(jwtToken);
            var timeStamp = _jwtManager.GetItemFromToken(jwtToken, "Timestamp");

            if (string.IsNullOrEmpty(userName) || !IsJwtTokenValid(userName + timeStamp, jwtToken))
            {
                if (_jwtManager.GetTokenTypeFromToken(jwtToken) == "refresh" && context.Request.Path.ToString().Contains("GetTokenByRefreshToken"))
                {
                    context.Request.EnableBuffering();

                    await _next(context);

                    context.Request.Body.Position = 0;

                    if (context.Response.StatusCode == 200)
                    {
                        await HandleTokenRefresh(context);
                    }
                }
                else
                {
                    context.Response.StatusCode = 401; // Unauthorized
                    await context.Response.WriteAsync("Invalid JWT token");
                    return;
                } 
            }
            else
            {
                context.Request.EnableBuffering();

                await _next(context);

                context.Request.Body.Position = 0;

                if (context.Response.StatusCode == 200)
                {
                    switch (context.Request.Path.ToString().ToLower())
                    {
                        case var path when path.Contains(_configuration["JWT:LogoutRoute"]!.ToLower()):
                            // Handle logout logic here
                            await HandleLogout(context);
                            break;

                        case var path when path.Contains(_configuration["JWT:UpdatePasswordRoute"]!.ToLower()):
                            // Handle update password logic here
                            await HandleUpdatePassword(context);
                            break;
                    }
                }

                return;

            }

        }

        private bool IsJwtTokenValid(string key, string token)
        {
            return _cachingManager.IsExists(key, token);
        }

        private async Task HandleLogin(HttpContext context)
        {
            var token = await ProcessRequestAndGenerateToken(context, _jwtManager.GenerateToken);
            SetTokenResponseHeaders(context, token);
        }

        private async Task HandleRegister(HttpContext context)
        {
            var token = await ProcessRequestAndGenerateToken(context, _jwtManager.RegisterAsync);
            SetTokenResponseHeaders(context, token);
        }

        private async Task HandleLogout(HttpContext context)
        {
            await _jwtManager.LogoutAsync(context.Request.Headers);
        }

        private async Task HandleUpdatePassword(HttpContext context)
        {
            var token = await _jwtManager.UpdatePassword(context.Request.Headers);
            SetTokenResponseHeaders(context, token);
        }

        private async Task HandleTokenRefresh(HttpContext context)
        {
            var token = await _jwtManager.GetTokenByRefreshTokenAsync(context.Request.Headers);

            if (token is null)
            {
                context.Response.StatusCode = 401; // Unauthorized
                await context.Response.WriteAsync("Invalid JWT token");
                return;
            }
            SetTokenResponseHeaders(context, token);
        }

        private async Task<TokenModel> ProcessRequestAndGenerateToken(HttpContext context, Func<LoginModel, Task<TokenModel>> generateTokenMethod)
        {
            string requestBody;
            using (var reader = new StreamReader(context.Request.Body))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            var requestData = JsonConvert.DeserializeObject<LoginModel>(requestBody);
            return await generateTokenMethod(requestData!);
        }

        private void SetTokenResponseHeaders(HttpContext context, TokenModel token)
        {
            // Get the properties of the TokenModel class
            var tokenProperties = typeof(TokenModel).GetProperties();

            // Iterate over the properties and add them to the response headers
            foreach (var property in tokenProperties)
            {
                var propertyName = property.Name;
                var propertyValue = property.GetValue(token);

                // Add the property to the response headers
                context.Response.Headers.Append(propertyName, propertyValue!.ToString());
            }
        }

    }

    public static class CustomJwtValidationMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomJwtValidation(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CustomJwtValidationManager>();
        }
    }
}
