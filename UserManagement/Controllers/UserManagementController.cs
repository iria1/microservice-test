using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;
using UserManagement.DBContexts;
using UserManagement.Models;

namespace UserManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserManagementController : ControllerBase
    {
        private readonly ILogger<UserManagementController> _logger;
        private readonly UserManagementDBContext _dbContext;

        public UserManagementController(ILogger<UserManagementController> logger, UserManagementDBContext context)
        {
            _logger = logger;
            _dbContext = context;
        }

        [HttpGet("GetUser")]
        [Authorize]
        public IActionResult GetUser()
        {
            string uidString = null;

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                uidString = identity.FindFirst("userId").Value;
            }

            var tpRes = long.TryParse(uidString, out long userId);

            UserManagementModel umm = new UserManagementModel(_dbContext);

            var result = umm.GetUser(new GetUserRequest { Id = userId });

            return Ok(result);
        }

        [HttpPost("CreateUser")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            UserManagementModel umm = new UserManagementModel(_dbContext);

            var result = await umm.CreateUser(request);

            return Ok(result);
        }
    }
}
