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
            AuthenticationModel am = new AuthenticationModel(_logger, _configuration, _dbContext);

            var result = am.Authenticate(request);

            if (result.IsSuccess == true)
            {
                return Ok(result);
            }
            else if (result.Message.Contains("unexpected"))
            {
                return StatusCode(500, result);
            }
            else
            {
                return StatusCode(401, result);
            }
        }

        [HttpPost("CreateNew")]
        [AllowAnonymous]
        public IActionResult CreateNew([FromBody] CreateNewRequest request)
        {
            if (_configuration["apiKey"] != Request.Headers["x-api-key"]) return StatusCode(403);

            AuthenticationModel am = new AuthenticationModel(_logger, _configuration, _dbContext);

            var result = am.CreateNew(request);

            if (result.IsSuccess == true)
            {
                return Ok(result);
            }
            else
            {
                return StatusCode(500, result);
            }
        }

        [HttpPost("ChangePassword")]
        [Authorize]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            string uidString = null;
            if (HttpContext.User.Identity is ClaimsIdentity identity) uidString = identity.FindFirst("userId").Value;
            var tpRes = long.TryParse(uidString, out long userId);
            if (!tpRes)
            {
                _logger.LogError($"Failed to parse {uidString}.");

                return StatusCode(500, "An unexpected error occurred. Please contact administrator.");
            }

            AuthenticationModel am = new AuthenticationModel(_logger, _configuration, _dbContext);

            request.UserId = userId;

            var result = am.ChangePassword(request);

            if (result.IsSuccess == true)
            {
                return Ok(result);
            }
            else
            {
                return StatusCode(500, result);
            }
        }

        [HttpDelete("DeleteAccount")]
        [AllowAnonymous]
        public IActionResult DeleteAccount([FromBody] DeleteAccountRequest request)
        {
            if (_configuration["apiKey"] != Request.Headers["x-api-key"]) return StatusCode(403);

            AuthenticationModel am = new AuthenticationModel(_logger, _configuration, _dbContext);

            var result = am.DeleteAccount(request);

            if (result.IsSuccess == true)
            {
                return Ok(result);
            }
            else
            {
                return StatusCode(500, result);
            }
        }

        [HttpPost("Refresh")]
        [AllowAnonymous]
        public IActionResult Refresh([FromBody] RefreshRequest request)
        {
            AuthenticationModel am = new AuthenticationModel(_logger, _configuration, _dbContext);

            var result = am.Refresh(request);

            if (result.IsSuccess == true)
            {
                return Ok(result);
            }
            else if (result.Message.Contains("unexpected"))
            {
                return StatusCode(500, result);
            }
            else
            {
                return StatusCode(401, result);
            }
        }

        [HttpPost("Logout")]
        [Authorize]
        public IActionResult Logout([FromBody] LogoutRequest request)
        {
            string uidString = null;
            if (HttpContext.User.Identity is ClaimsIdentity identity) uidString = identity.FindFirst("userId").Value;
            var tpRes = long.TryParse(uidString, out long userId);
            if (!tpRes)
            {
                _logger.LogError($"Failed to parse {uidString}.");

                return StatusCode(500, "An unexpected error occurred. Please contact administrator.");
            }

            AuthenticationModel am = new AuthenticationModel(_logger, _configuration, _dbContext);

            request.UserId = userId;

            var result = am.Logout(request);

            if (result.IsSuccess == true)
            {
                return Ok(result);
            }
            else
            {
                return StatusCode(500, result);
            }
        }

        // This endpoint literally does nothing.
        // It's only used to check if a user has been authorized.
        [HttpGet("DoStuff")]
        [Authorize]
        public IActionResult DoStuff()
        {
            return Ok(new { message = "doing stuff" });
        }
    }
}
