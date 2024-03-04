using ExperimentWebApp.Entities;
using JwtNugetClassLibrary.Manager;
using JwtNugetClassLibrary.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExperimentWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtExperimentNugetDbContext _dbContext;
        public AuthController(JwtExperimentNugetDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            #region Save to db
            UserInfo userInfo = new UserInfo();
            userInfo.UserName = model.Username;
            userInfo.Password = model.Password;
            userInfo.Email = model.Email;
            _dbContext.Add(userInfo);
            _dbContext.SaveChanges();
            #endregion

            return Ok();
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (_dbContext.UserInfos.Any(x => x.UserName == model.Username && x.Password == model.Password))
            {
                return Ok();
            }
            return Unauthorized();
        }

        [Authorize]
        [HttpPost]
        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            return Ok();
        }

        [Authorize]
        [HttpGet]
        [Route("GetTokenByRefreshToken")]
        public async Task<IActionResult> GetTokenByRefreshToken()
        {
            return Ok();
        }

        [Authorize]
        [HttpPost]
        [Route("UpdatePassword")]
        public async Task<IActionResult> UpdatePassword([FromBody] LoginModel model)
        {
            #region Update to db
            var userInfo = _dbContext.UserInfos.SingleOrDefault(x => x.UserName == model.Username);
            if (userInfo is not null)
            {
                userInfo.Password = model.Password;
                await _dbContext.SaveChangesAsync();
            }
            #endregion

            return Ok();
        }
    }
}
