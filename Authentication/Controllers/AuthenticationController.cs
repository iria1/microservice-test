using Authentication.Models;
using EFCoreMySQL.DBContexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Authentication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly ILogger<AuthenticationController> _logger;
        private readonly MyDBContext myDbContext;

        public AuthenticationController(ILogger<AuthenticationController> logger, MyDBContext context)
        {
            myDbContext = context;
            _logger = logger;
        }

        [HttpPost("Authenticate")]
        public IActionResult Authenticate([FromBody] AuthenticateRequest request)
        {
            AuthenticationModel am = new AuthenticationModel(myDbContext);

            var result = am.Authenticate(request);

            return Ok(result);
        }
    }
}
