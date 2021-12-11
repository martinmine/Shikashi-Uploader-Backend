using Microsoft.AspNetCore.Http;
using ShikashiAPI.Model;
using ShikashiAPI.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ShikashiAPI.AuthenticatorMiddleware
{
    /// <summary>
    /// Middleware that intercepts calls and reads a bearer token and authenticates the user based on this token.
    /// </summary>
    public class TokenAuthenticatorMiddleware
    {
        private RequestDelegate next;

        /// <summary>
        /// Creates a new authenticator middleware.
        /// </summary>
        /// <param name="next">Next middleware</param>
        /// <param name="persistenceProvider">Persistence provider</param>
        public TokenAuthenticatorMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        /// <summary>
        /// Invokes the middleware, gets the authorization bearer token and tries to find an existing token in the database
        /// for then to set the ClaimsPrincipal for this request.
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context, IKeyService keyService)
        {
            string authorizationHeader = context.Request.Headers["Authorization"];

            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.Contains("-"))
            {
                APIKey key = await keyService.GetKey(authorizationHeader);
                context.User = new ClaimsPrincipal(new ShikashiUploaderAPIUser(key));
            }

            await next.Invoke(context);
        }
    }
}
