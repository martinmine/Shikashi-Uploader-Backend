using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using ShikashiAPI.Model;
using System.Threading.Tasks;

namespace ShikashiAPI.Policies
{
    public class UserAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, APIKey>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, APIKey resource)
        {
            if (resource?.Compose() != context?.User?.Identity?.Name)
            {
                context.Fail();
            }

            context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
