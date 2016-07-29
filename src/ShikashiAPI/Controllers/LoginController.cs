using Microsoft.AspNetCore.Mvc;
using ShikashiAPI.Model;
using ShikashiAPI.Services;
using ShikashiAPI.ViewModels;
using System.Threading.Tasks;

namespace ShikashiAPI.Controllers
{
    [Route("/login")]
    public class LoginController : Controller
    {
        private IKeyService keyService;
        private IUserService userService;

        public LoginController(IKeyService keyService, IUserService userService)
        {
            this.keyService = keyService;
            this.userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> LoginUser([FromForm] string email, [FromForm] string password, [FromForm] string client)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return BadRequest();
            }

            var user = await userService.LoginUser(email, password);

            if (user == null)
            {
                return NotFound();
            }
            
            APIKey key = await keyService.CreateKey(user, client == "Shikashi-Win32");

            APIKeyViewModel viewModel = new APIKeyViewModel()
            {
                Key = $"{key.Id}-{key.Identifier}",
                ExpirationTime = key.ExpirationTime
            };

            return Ok(viewModel);
        }
    }
}
