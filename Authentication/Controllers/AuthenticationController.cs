using Authentication.DBContexts;
using Authentication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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

            return Ok(result);
        }

        [HttpPost("CreateNew")]
        [AllowAnonymous]
        public IActionResult CreateNew([FromBody] CreateNewRequest request)
        {
            AuthenticationModel am = new AuthenticationModel(_configuration, _dbContext);

            var result = am.CreateNew(request);

            return Ok(result);
        }
    }
}
