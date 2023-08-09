using Authentication.Models;
using Authentication.DBContexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Authentication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly ILogger<AuthenticationController> _logger;
        private readonly AuthenticationDBContext _dbContext;

        public AuthenticationController(ILogger<AuthenticationController> logger, AuthenticationDBContext context)
        {
            _dbContext = context;
            _logger = logger;
        }

        [HttpPost("Authenticate")]
        public IActionResult Authenticate([FromBody] AuthenticateRequest request)
        {
            AuthenticationModel am = new AuthenticationModel(_dbContext);

            var result = am.Authenticate(request);

            return Ok(result);
        }

        [HttpPost("CreateNew")]
        public IActionResult CreateNew([FromBody] CreateNewRequest request)
        {
            AuthenticationModel am = new AuthenticationModel(_dbContext);

            am.CreateNew(request);

            return Ok();
        }
    }
}
