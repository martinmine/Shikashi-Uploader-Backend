using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace ShikashiAPI.Policies
{
    /// <summary>
    /// Some of this code has been gathered from:
    ///     http://docs.asp.net/en/latest/security/authorization/resourcebased.html
    ///     
    /// Defines basic CRUD authorization requirements.
    /// </summary>
    public static class Operations
    {
        public static OperationAuthorizationRequirement Create =
            new OperationAuthorizationRequirement { Name = "Create" };
        public static OperationAuthorizationRequirement Read =
            new OperationAuthorizationRequirement { Name = "Read" };
        public static OperationAuthorizationRequirement Update =
            new OperationAuthorizationRequirement { Name = "Update" };
        public static OperationAuthorizationRequirement Delete =
            new OperationAuthorizationRequirement { Name = "Delete" };
    }
}
