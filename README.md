# JWT Token Management NuGet Package

## Overview
This repository contains a NuGet package for managing JSON Web Tokens (JWT) within ASP.NET Core applications. The package provides various functionalities for JWT-based authentication and authorization, including token generation, refresh token handling, and database integration.

## Features
- **JWT Token Generation:** Generate JWT tokens with customizable configurations.
- **Refresh Token Handling:** Implement secure refresh token mechanisms to extend token validity.
- **Database Integration:** Store access tokens in an in-memory database and refresh tokens in a persistent database.
- **AppSettings Configuration:** Configure the package behavior through the `appsettings.json` file, allowing customization of token validity, database connections, and route configurations.
- **API Endpoints:** Predefined API endpoints for full-cycle JWT authentication, including user registration, login, logout, token refreshing, and password updates.

## Example AppSettings Configuration
```json
{
  "JWT": {
    "ValidAudience": "http://nuran.com",
    "ValidIssuer": "http://nuran.com",
    "Secret": "JWTRefreshTokenHIGHsecuredPasswordVVVp1OH7Xzyr",
    "TokenValidityInMinutes": 5,
    "RefreshTokenValidityInDays": 7,
    "ConnectionString": "Server=DESKTOP-T9S3VKL;Database=TestDB;Integrated Security=True;Trusted_Connection=True;TrustServerCertificate=True;",
    "UserTableName": "AuthUser",
    "UserTablePrimaryKey": "Id",
    "UserTableUniqueName": "UniqueName",
    "JwtTableName": "TokenStorage",
    "RegisterRoute": "auth/register",
    "LoginRoute": "auth/login",
    "LogoutRoute": "auth/logout",
    "TokenRoute": "auth/GetTokenByRefreshToken",
    "UpdatePasswordRoute": "auth/UpdatePassword"
  }
}
