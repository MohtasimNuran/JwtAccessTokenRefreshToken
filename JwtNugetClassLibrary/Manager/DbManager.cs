using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JwtNugetClassLibrary.Manager
{
    public class DbManager
    {
        private readonly IConfiguration _configuration;
        private readonly IDbConnection _dbConnection;
        private readonly string _jwtTableName;
        private readonly string _userTableName;
        private readonly string _userTablePrimaryKey;
        private readonly string _userTableUniqueName;

        public DbManager(IConfiguration configuration)
        {
            _configuration = configuration;
            _dbConnection = new SqlConnection(_configuration["JWT:ConnectionString"]);
            _jwtTableName = _configuration["JWT:JwtTableName"]!;
            _userTableName = _configuration["JWT:UserTableName"]!;
            _userTablePrimaryKey = _configuration["JWT:UserTablePrimaryKey"]!;
            _userTableUniqueName = _configuration["JWT:UserTableUniqueName"]!;
        }

        public async Task CreateTable()
        {
            var script = $@"
                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{_jwtTableName}')
                        BEGIN
                        CREATE TABLE {_jwtTableName}
                        (
                            {_jwtTableName}Id INT PRIMARY KEY IDENTITY(1,1),
                            {_userTablePrimaryKey} INT NOT NULL,
                            RefreshToken NVARCHAR(MAX) NOT NULL,
                            Timestamp DATETIME DEFAULT GETDATE() NOT NULL
                        );
                        END;";

            await _dbConnection.ExecuteAsync(script);

        }

        public async Task InsertToTokenTable(string userName, string refreshToken)
        {
            var insertQuery = $@"
            INSERT INTO TokenStorage ({_userTablePrimaryKey}, RefreshToken)
            VALUES ((SELECT TOP 1 {_userTablePrimaryKey} FROM {_userTableName} u WHERE u.{_userTableUniqueName} = @UserName), @RefreshToken);";

            var parameters = new
            {
                UserName = userName,
                RefreshToken = ""
            };

            await _dbConnection.ExecuteAsync(insertQuery, parameters);

        }

        public async Task UpdateRefreshToken(string userName, string refreshToken)
        {
            string updateQuery = $@"UPDATE {_jwtTableName}
                            SET RefreshToken = @RefreshToken
                            WHERE {_userTablePrimaryKey} = (SELECT {_userTablePrimaryKey} FROM {_userTableName} WHERE {_userTableUniqueName} = @UserName)";

            var parameters = new { RefreshToken = refreshToken, UserName = userName };

            await _dbConnection.ExecuteAsync(updateQuery, parameters);
        }

        public async Task<bool> IsTokenExists(string userName, string token)
        {
            string selectQuery = $@"SELECT 1
                                FROM {_jwtTableName} jt
                                INNER JOIN {_userTableName} u ON jt.{_userTablePrimaryKey} = u.{_userTablePrimaryKey}
                                WHERE u.{_userTableUniqueName} = @UserName AND jt.RefreshToken = @Token";

            var parameters = new { UserName = userName, Token = token };

            return await _dbConnection.QueryFirstOrDefaultAsync<bool>(selectQuery, parameters);
        }
    }
}
