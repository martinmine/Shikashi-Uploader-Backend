using ShikashiAPI.Model;
using System.Security.Principal;

namespace ShikashiAPI.AuthenticatorMiddleware
{
    /// <summary>
    /// Declares an identify for an authorized API user such that ASP.NET can understand the user.
    /// </summary>
    public class ShikashiUploaderAPIUser : IIdentity
    {
        private APIKey credentials;

        /// <summary>
        /// Specifies that this user is authenticated using a bearer token.
        /// </summary>
        public string AuthenticationType
        {
            get
            {
                return "Token";
            }
        }

        /// <summary>
        /// Indicates whether the user is authenticated or not.
        /// </summary>
        public bool IsAuthenticated
        {
            get
            {
                return credentials != null;
            }
        }

        /// <summary>
        /// Name of the user that is authenticated.
        /// </summary>
        public string Name
        {
            get
            {
                if (credentials != null)
                {
                    return credentials.Compose();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Creates a new user using existing credentials.
        /// </summary>
        /// <param name="credentials">Credentials for the user</param>
        public ShikashiUploaderAPIUser(APIKey credentials)
        {
            this.credentials = credentials;
        }
    }
}
