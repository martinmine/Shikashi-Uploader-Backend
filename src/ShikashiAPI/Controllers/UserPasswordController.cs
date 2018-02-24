using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShikashiAPI.Policies;
using ShikashiAPI.Services;
using System.Threading.Tasks;
using Serilog;

namespace ShikashiAPI.Controllers
{
    [Route("/account/password")]
    public class UserPasswordController : UserAuthenticatedController
    {
        private readonly IUserService _userService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger _logger;

        public UserPasswordController(IUserService userService, IKeyService keyService, IAuthorizationService authService, ILogger logger)
            : base(keyService)
        {
            _userService = userService;
            _authorizationService = authService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromForm] string currentPassword, [FromForm] string newPassword)
        {
            var key = await GetCurrentKey();
            if (!(await _authorizationService.AuthorizeAsync(User, key, Operations.Create)).Succeeded)
            {
                return new ChallengeResult();
            }

            var updatedUser = await _userService.SetUserPassword(key.User.Email, currentPassword, newPassword);
            if (updatedUser == null)
            {
                return new StatusCodeResult(406);
            }

            _logger.Information("Changed password for user {@User} on IP {@IP}", key.User, HttpContext.Connection.RemoteIpAddress);

            return Ok();
        }
    }
}
