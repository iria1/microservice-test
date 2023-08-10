using Authentication.DBContexts;
using Authentication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Authentication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly ILogger<AuthenticationController> _logger;
        private readonly AuthenticationDBContext _dbContext;
        private readonly IConfiguration _configuration;

        public AuthenticationController(IConfiguration configuration, ILogger<AuthenticationController> logger, AuthenticationDBContext context)
        {
            _configuration = configuration;
            _dbContext = context;
            _logger = logger;
        }

        [HttpPost("Authenticate")]
        [AllowAnonymous]
        public IActionResult Authenticate([FromBody] AuthenticateRequest request)
        {
            AuthenticationModel am = new AuthenticationModel(_configuration, _dbContext);

            var result = am.Authenticate(request);

            if (result != null)
            {
                if (result.token != null)
                {
                    return Ok(result);
                }
                else
                {
                    return StatusCode(401, new { message = "Wrong username/password combination." });
                }
            }
            else
            {
                return StatusCode(500, new { message = "An unexpected error occurred. Please contact administrator." });
            }
        }

        [HttpPost("CreateNew")]
        [AllowAnonymous]
        public IActionResult CreateNew([FromBody] CreateNewRequest request)
        {
            var apiKey = Request.Headers["x-api-key"];

            if (_configuration["apiKey"] != apiKey) return StatusCode(403);

            AuthenticationModel am = new AuthenticationModel(_configuration, _dbContext);

            var result = am.CreateNew(request);

            return Ok(result);
        }

        [HttpPost("ChangePassword")]
        [Authorize]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            string uidString = null;
            if (HttpContext.User.Identity is ClaimsIdentity identity) uidString = identity.FindFirst("userId").Value;
            var tpRes = long.TryParse(uidString, out long userId);
            if (!tpRes) return StatusCode(500, "An unexpected error occurred. Please contact administrator.");
            
            AuthenticationModel am = new AuthenticationModel(_configuration, _dbContext);

            request.UserId = userId;

            var result = am.ChangePassword(request);

            return Ok(result);
        }

        [HttpDelete("DeleteAccount")]
        [AllowAnonymous]
        public IActionResult DeleteAccount([FromBody] DeleteAccountRequest request)
        {
            var apiKey = Request.Headers["x-api-key"];

            if (_configuration["apiKey"] != apiKey) return StatusCode(403);

            AuthenticationModel am = new AuthenticationModel(_configuration, _dbContext);

            var result = am.DeleteAccount(request);

            return Ok(result);
        }
    }
}
