using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using PrismaApi.Domain.Constants;
using System.Security.Claims;

namespace PrismaApi.Api.SecurityPolicy;

public class AppRolesPolicy
{
    public const string UserRoleRequired = "UserRoleRequired";
    public static readonly string ReadScope = "Read";

    private static bool HandleCustomAuthorization(AuthorizationHandlerContext context)
    {
        var acceptedScope = ReadScope;

        Claim? scopeClaim = context?.User?.FindFirst(ClaimConstants.Scp)
            ?? context?.User?.FindFirst(ClaimConstants.Scope);

        // Check for appid and roles claims
        Claim? appidClaim = context?.User?.FindFirst("appid");
        Claim? rolesClaim = context?.User?.FindFirst("roles");


        if (scopeClaim == null)
        {
            scopeClaim = context?.User?.FindFirst(ClaimConstants.Scp);
        }
        var scopes = scopeClaim?.Value.Split(' ');
        var hasScope = scopes?.Where(scope => scope == acceptedScope).Any() ?? false;
        return hasScope;
    }

    public static void AddPrismaDecisionUserPolicy(AuthorizationPolicyBuilder policy)
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole(AppRoles.PrismaDecisionUser)
            .RequireAssertion(context => HandleCustomAuthorization(context));
    }


}
