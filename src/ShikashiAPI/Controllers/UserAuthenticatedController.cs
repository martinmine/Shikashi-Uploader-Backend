using Microsoft.AspNetCore.Mvc;
using ShikashiAPI.Model;
using ShikashiAPI.Services;
using System.Threading.Tasks;

namespace ShikashiAPI.Controllers
{
    public class UserAuthenticatedController : Controller
    {
        private readonly IKeyService _keyService;

        public UserAuthenticatedController(IKeyService keyService)
        {
            _keyService = keyService;
        }

        public async Task<APIKey> GetCurrentKey()
        {
            string authorizationToken = HttpContext.Request.Headers["Authorization"];
            return await _keyService.GetKey(authorizationToken);
        }
    }
}
