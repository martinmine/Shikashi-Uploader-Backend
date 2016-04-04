using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using ShikashiAPI.Policies;
using ShikashiAPI.Services;
using System.Threading.Tasks;

namespace ShikashiAPI.Controllers
{
    [Route("/account/password")]
    public class UserPasswordController : UserAuthenticatedController
    {
        private IUserService userService;
        private IAuthorizationService authorizationService;

        public UserPasswordController(IUserService userService, IKeyService keyService, IAuthorizationService authService)
            : base(keyService)
        {
            this.userService = userService;
            this.authorizationService = authService;
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromForm] string currentPassword, [FromForm] string newPassword)
        {
            var key = await GetCurrentKey();
            if (!await authorizationService.AuthorizeAsync(User, key, Operations.Create))
            {
                return new ChallengeResult();
            }

            var updatedUser = userService.SetUserPassword(key.User.Email, currentPassword, newPassword);
            if (updatedUser == null)
            {
                return new HttpStatusCodeResult(406);
            }

            return Ok();
        }
    }
}
