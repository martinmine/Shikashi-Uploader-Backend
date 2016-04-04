using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Authorization.Infrastructure;
using ShikashiAPI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShikashiAPI.Policies
{
    public class UserAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, APIKey>
    {
        protected override void Handle(AuthorizationContext context, OperationAuthorizationRequirement requirement, APIKey resource)
        {
            if (resource?.Compose() != context?.User?.Identity?.Name)
            {
                context.Fail();
            }

            context.Succeed(requirement);
        }
    }
}
