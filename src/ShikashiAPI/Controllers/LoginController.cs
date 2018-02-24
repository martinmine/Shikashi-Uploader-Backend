using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ShikashiAPI.Model;
using ShikashiAPI.Services;
using ShikashiAPI.ViewModels;
using System.Threading.Tasks;

namespace ShikashiAPI.Controllers
{
    [Route("/login")]
    public class LoginController : Controller
    {
        private readonly IKeyService _keyService;
        private readonly IUserService _userService;
        private readonly ILogger<LoginController> _logger;

        public LoginController(IKeyService keyService, IUserService userService, ILogger<LoginController> logger)
        {
            _keyService = keyService;
            _userService = userService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> LoginUser([FromForm] string email, [FromForm] string password, [FromForm] string client)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return BadRequest();
            }

            var user = await _userService.LoginUser(email, password);

            if (user == null)
            {
                _logger.LogInformation("Failed login attempt for email {@Email} by ip {@Ip} on {@Client}",
                    email, HttpContext.Connection.RemoteIpAddress, client);
                return NotFound();
            }
            
            APIKey key = await _keyService.CreateKey(user, client == "Shikashi-Win32");

            APIKeyViewModel viewModel = new APIKeyViewModel()
            {
                Key = $"{key.Id}-{key.Identifier}",
                ExpirationTime = key.ExpirationTime
            };

            _logger.LogInformation("Login succeeded for email {@Email} by ip {@Ip} on {@Client}",
                email, HttpContext.Connection.RemoteIpAddress, client);

            return Ok(viewModel);
        }
    }
}
