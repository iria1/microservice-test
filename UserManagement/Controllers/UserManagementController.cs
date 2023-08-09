using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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

        [HttpPost("GetUser")]
        public IActionResult GetUser([FromBody] GetUserRequest request)
        {
            UserManagementModel umm = new UserManagementModel(_dbContext);

            var result = umm.GetUser(request);

            return Ok(result);
        }
    }
}
