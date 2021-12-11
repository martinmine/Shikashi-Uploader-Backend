using Microsoft.AspNetCore.Mvc;

namespace ShikashiAPI.Controllers
{
    [Route("/app")]
    public class AppController : Controller
    {
        [HttpGet]
        public IActionResult GetApp()
        {
            return Ok("Ey b0ss");
        }
    }
}
