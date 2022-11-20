using aspnetserver.Auth.Model;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace aspnetserver.Auth;

public class ResourceOwnerAuthorizationHandler : AuthorizationHandler<ResourceOwnerRequirement, IUserOwnedResource>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ResourceOwnerRequirement requirement,
        IUserOwnedResource resource)
    {
        if (context.User.IsInRole(WarehouseRoles.Admin) ||
            context.User.FindFirstValue(JwtRegisteredClaimNames.Sub) == resource.UserId/* || context.User.IsInRole(WarehouseRoles.Worker)*/)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public record ResourceOwnerRequirement : IAuthorizationRequirement;
