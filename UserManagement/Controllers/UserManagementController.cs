using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;
using UserManagement.DBContexts;
using UserManagement.Models;
using Microsoft.Extensions.Configuration;

namespace UserManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserManagementController : ControllerBase
    {
        private readonly ILogger<UserManagementController> _logger;
        private readonly UserManagementDBContext _dbContext;
        private readonly IConfiguration _configuration; 

        public UserManagementController(ILogger<UserManagementController> logger, UserManagementDBContext context, IConfiguration configuration)
        {
            _logger = logger;
            _dbContext = context;
            _configuration = configuration;
        }

        [HttpGet("GetUser")]
        [Authorize]
        public IActionResult GetUser()
        {
            string uidString = null;
            if (HttpContext.User.Identity is ClaimsIdentity identity) uidString = identity.FindFirst("userId").Value;
            var tpRes = long.TryParse(uidString, out long userId);
            if (!tpRes) return StatusCode(500, "An unexpected error occurred. Please contact administrator.");

            UserManagementModel umm = new UserManagementModel(_logger, _dbContext, _configuration);

            var result = umm.GetUser(new GetUserRequest { Id = userId });

            if (result.IsSuccess == true)
            {
                return Ok(result);
            }
            else
            {
                return StatusCode(500, result);
            }
        }

        [HttpPost("CreateUser")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            UserManagementModel umm = new UserManagementModel(_logger, _dbContext, _configuration);

            var result = await umm.CreateUser(request);

            if (result.IsSuccess == true)
            {
                return Ok(result);
            }
            else
            {
                return StatusCode(500, result);
            }
        }

        [HttpPost("ModifyUser")]
        [Authorize]
        public IActionResult ModifyUser([FromBody] ModifyUserRequest request)
        {
            string uidString = null;
            if (HttpContext.User.Identity is ClaimsIdentity identity) uidString = identity.FindFirst("userId").Value;
            var tpRes = long.TryParse(uidString, out long userId);
            if (!tpRes) return StatusCode(500, "An unexpected error occurred. Please contact administrator.");

            request.UserId = userId;

            UserManagementModel umm = new UserManagementModel(_logger, _dbContext, _configuration);

            var result = umm.ModifyUser(request);

            if (result.IsSuccess == true)
            {
                return Ok(result);
            }
            else
            {
                return StatusCode(500, result);
            }
        }

        [HttpDelete("DeleteUser")]
        [Authorize]
        public async Task<IActionResult> DeleteUser()
        {
            string uidString = null;
            if (HttpContext.User.Identity is ClaimsIdentity identity) uidString = identity.FindFirst("userId").Value;
            var tpRes = long.TryParse(uidString, out long userId);
            if (!tpRes) return StatusCode(500, "An unexpected error occurred. Please contact administrator.");

            UserManagementModel umm = new UserManagementModel(_logger, _dbContext, _configuration);

            var result = await umm.DeleteUser(userId);

            if (result.IsSuccess == true)
            {
                return Ok(result);
            }
            else
            {
                return StatusCode(500, result);
            }
        }
    }
}
