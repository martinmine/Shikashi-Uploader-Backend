using Microsoft.AspNet.Mvc;
using ShikashiAPI.Services;
using System.Threading.Tasks;

namespace ShikashiAPI.Controllers
{
    [Route("/register")]
    public class RegisterController : Controller
    {
        private IUserService userService;

        public RegisterController(IUserService userService)
        {
            this.userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterUser([FromForm] string email, [FromForm] string password, [FromForm] string inviteKey)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(inviteKey))
            {
                return HttpBadRequest();
            }

            if (password.Length < 5)
            {
                return new HttpStatusCodeResult(406);
            }

            if (await userService.GetUser(email) != null)
            {
                return new HttpStatusCodeResult(409);
            }

            var registeredUser = await userService.RegisterUser(email, password, inviteKey);

            if (registeredUser == null)
            {
                return new HttpStatusCodeResult(403);
            }

            return Created("/", null);
        }
    }
}
