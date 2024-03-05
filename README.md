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
    "ValidAudience": "http://yoursite.com",
    "ValidIssuer": "http://yoursite.com",
    "Secret": "JWTRefreshTokenHIGHsecuredPasswordVVVp1OH7Xzyr",
    "TokenValidityInMinutes": 5,
    "RefreshTokenValidityInDays": 7,
    "ConnectionString": "Server=<server_name>;Database=<db_name>;Integrated Security=True;Trusted_Connection=True;TrustServerCertificate=True;",
    "UserTableName": "AuthUser_Table", ///Table, you are already using to save user data like, AuthUser_Table
    "UserTablePrimaryKey": "Id", ///unique Id in AuthUser_Table
    "UserTableUniqueName": "UniqueName", ///unique username/email in AuthUser_Table
    "JwtTableName": "TokenStorage", ///This nuget package is going to create a table to store token, TokenStorage_Table
    "RegisterRoute": "auth/Register", ///This is the route to register new user
    "LoginRoute": "auth/Login", ///This is the route to login
    "LogoutRoute": "auth/Logout", ///This is the route to logout
    "TokenRoute": "auth/GetTokenByRefreshToken", ///This is the route you need to create for fetching Refresh token
    "UpdatePasswordRoute": "auth/UpdatePassword" ///This is the route to update/edit password
  }
}
```
## Example AuthController
```c#

[HttpPost]
[Route("auth/Register")]
public async Task<IActionResult> Register([FromBody] RegisterModel model)
{
    ///NOTE: Your Registration logic
    return Ok();
}

[HttpPost]
[Route("auth/Login")]
public async Task<IActionResult> Login([FromBody] LoginModel model)
{
    if (IsValidLogin())
    {
        return Ok();
    }
    return Unauthorized();
}

[Authorize] ///NOTE: We need to use [Authorize]
[HttpPost]
[Route("auth/Logout")]
public async Task<IActionResult> Logout()
{
    return Ok();
}

[Authorize] ///NOTE: We need to use [Authorize]
[HttpGet]
[Route("auth/GetTokenByRefreshToken")]
public async Task<IActionResult> GetTokenByRefreshToken()
{
    ///NOTE: Implement your logic if any
    return Ok();
}

[Authorize] ///NOTE: We need to use [Authorize]
[HttpPost]
[Route("auth/UpdatePassword")]
public async Task<IActionResult> UpdatePassword([FromBody] LoginModel model)
{
    ///NOTE: Implement your logic
    return Ok();
}