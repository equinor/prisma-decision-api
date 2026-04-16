using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Constants;

namespace PrismaApi.Api.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class LoadUser : Attribute, IAsyncActionFilter
{
    public const string CurrentUserKey = AppConstants.CurrentUserKey;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<LoadUser>>();

        try
        {
            var user = await userService.GetOrCreateUserFromContextAsync(context.HttpContext);
            context.HttpContext.Items[CurrentUserKey] = user;
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Failed to resolve user for request : {Message}", ex.Message);
            context.Result = new BadRequestObjectResult(new { error = ex.Message });
            return;
        }

        await next();
    }
}
