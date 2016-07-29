using Microsoft.AspNetCore.Mvc;
using ShikashiAPI.Model;
using ShikashiAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShikashiAPI.Controllers
{
    public class UserAuthenticatedController : Controller
    {
        public IKeyService keyService;

        public UserAuthenticatedController(IKeyService keyService)
        {
            this.keyService = keyService;
        }

        public async Task<APIKey> GetCurrentKey()
        {
            string authorizationToken = HttpContext.Request.Headers["Authorization"];
            return await keyService.GetKey(authorizationToken);
        }
    }
}
