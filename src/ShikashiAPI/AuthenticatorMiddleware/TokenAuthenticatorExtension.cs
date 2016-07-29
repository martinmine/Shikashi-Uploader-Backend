using Microsoft.AspNetCore.Builder;

namespace ShikashiAPI.AuthenticatorMiddleware
{
    /// <summary>
    /// Extension for applications that wants to use the token authenticator middleware.
    /// </summary>
    public static class TokenAuthenticatorExtension
    {
        /// <summary>
        /// Specifies that requests should be authenticated using a bearer token middleware handler.
        /// </summary>
        /// <param name="builder">Application builder</param>
        /// <returns>Application builder</returns>
        public static IApplicationBuilder UseTokenAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TokenAuthenticatorMiddleware>();
        }
    }
}
