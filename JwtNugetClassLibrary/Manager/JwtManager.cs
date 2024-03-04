using JwtNugetClassLibrary.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace JwtNugetClassLibrary.Manager
{
    public class JwtManager
    {
        private readonly IConfiguration _configuration;
        private readonly CachingManager _cachingManager;
        private readonly DbManager _dbManager;
        public JwtManager(IConfiguration configuration, CachingManager cachingManager, DbManager dbManager)
        {
            _configuration = configuration;
            _cachingManager = cachingManager;
            _dbManager = dbManager;
        }

        public Task<TokenModel> GenerateToken(LoginModel model)
        {
            return Task.FromResult(new TokenModel
            {
                Token = CreateToken(model),
                RefreshToken = CreateRefreshToken(model)
            });
        }

        public string CreateToken(LoginModel model)
        {
            var timeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.Username),
                    new Claim(ClaimTypes.Role, "admin"),
                    new Claim("TokenType", "access"),
                    new Claim("Timestamp", timeStamp),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!));
            var tokenValidityInMinutes = Convert.ToInt16(_configuration["JWT:TokenValidityInMinutes"]);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddMinutes(tokenValidityInMinutes),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            ///cahced for 5 mins
            _cachingManager.Set(model.Username + timeStamp, tokenString);

            return tokenString;
        }

        public string CreateRefreshToken(LoginModel model)
        {
            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.Username),
                    new Claim(ClaimTypes.Role, "admin"),
                    new Claim("TokenType", "refresh"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!));
            var tokenValidityInDays = Convert.ToInt16(_configuration["JWT:RefreshTokenValidityInDays"]);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddDays(tokenValidityInDays),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            /// save to db
            Task.Run(() => SaveRefreshTokenToDbAsync(model.Username, tokenString));

            return tokenString;
        }

        public async Task SaveRefreshTokenToDbAsync(string userName, string refreshToken)
        {
            await _dbManager.UpdateRefreshToken(userName, refreshToken);
        }

        public string? GetUserNameFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();

            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            var userName = jsonToken?.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Name)?.Value;

            return userName;
        }

        public string? GetTokenTypeFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();

            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            var tokenType = jsonToken?.Claims.SingleOrDefault(x => x.Type == "TokenType")?.Value;

            return tokenType;
        }

        public string? GetItemFromToken(string token, string claimType)
        {
            var handler = new JwtSecurityTokenHandler();

            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            var item = jsonToken?.Claims.SingleOrDefault(x => x.Type == claimType)?.Value;

            return item;
        }

        public async Task<TokenModel?> GetTokenByRefreshTokenAsync(IHeaderDictionary header)
        {
            try
            {
                var requestedToken = header["Authorization"].ToString().Replace("Bearer ", "");

                var userName = GetUserNameFromToken(requestedToken);

                var tokenType = GetTokenTypeFromToken(requestedToken);

                var isTokenExistsInDB = await _dbManager.IsTokenExists(userName!, requestedToken);

                if (!string.IsNullOrEmpty(userName) && tokenType == "refresh" && isTokenExistsInDB == true)
                {
                    var token = await GenerateToken(new LoginModel
                    {
                        Username = userName,
                        Password = "",
                    });

                    return token;
                }
            }
            catch (Exception)
            {
            }
            return null;
        }

        public async Task<TokenModel> UpdatePassword(IHeaderDictionary header)
        {
            var requestedToken = header["Authorization"].ToString().Replace("Bearer ", "");

            var timeStamp = GetItemFromToken(requestedToken, "Timestamp");
            var userName = GetUserNameFromToken(requestedToken);

            _cachingManager.RemoveBykey(userName + timeStamp);

            _cachingManager.RemoveItemsByKeyContains(userName!);

            var token = await GenerateToken(new LoginModel { Username = userName!, Password = "" });

            return token;
        }

        public async Task LogoutAsync(IHeaderDictionary header)
        {
            var requestedToken = header["Authorization"].ToString().Replace("Bearer ", "");

            if (!string.IsNullOrEmpty(requestedToken))
            {
                var timeStamp = GetItemFromToken(requestedToken, "Timestamp");
                var userName = GetUserNameFromToken(requestedToken);

                if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(timeStamp))
                {
                    _cachingManager.RemoveBykey(userName + timeStamp);
                    await _dbManager.UpdateRefreshToken(userName, "");
                }
            }

        }

        public async Task<TokenModel> RegisterAsync(LoginModel model)
        {
            await _dbManager.CreateTable();

            await _dbManager.InsertToTokenTable(model.Username, "");

            var token = await GenerateToken(new LoginModel
            {
                Username = model.Username,
                Password = ""
            });

            return token;
        }
    }
}
