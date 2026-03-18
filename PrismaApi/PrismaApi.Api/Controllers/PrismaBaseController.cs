using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrismaApi.Api.Attributes;

namespace PrismaApi.Api.Controllers;

[ApiController]
[LoadUser]
[Authorize(Policy = SecurityPolicy.UserRoleRequired)]
public class PrismaBaseController: ControllerBase
{
    protected string? GetUserCacheKeyFromClaims()
    {
        return User.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
    }
}
