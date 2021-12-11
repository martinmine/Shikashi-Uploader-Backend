using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ShikashiAPI.Services;
using System.Threading.Tasks;

namespace ShikashiAPI.Controllers
{
    [Route("/register")]
    public class RegisterController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<RegisterController> _logger;

        public RegisterController(IUserService userService, ILogger<RegisterController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterUser([FromForm] string email, [FromForm] string password, [FromForm] string inviteKey)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(inviteKey))
            {
                return BadRequest();
            }

            if (password.Length < 5)
            {
                return new StatusCodeResult(406);
            }

            if (await _userService.GetUser(email) != null)
            {
                return new StatusCodeResult(409);
            }

            var registeredUser = await _userService.RegisterUser(email, password, inviteKey);

            if (registeredUser == null)
            {
                return new StatusCodeResult(403);
            }

            _logger.LogInformation("Registered user with email {@Email} and inv key {@InviteKey}", email, inviteKey);

            return Created("/", null);
        }
    }
}
