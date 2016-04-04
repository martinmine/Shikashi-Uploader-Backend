using Microsoft.AspNet.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
