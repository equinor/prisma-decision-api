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
        // Get the UserService from DI
        var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();

        var user = await userService.GetOrCreateUserFromContextAsync(context.HttpContext);

        context.HttpContext.Items[CurrentUserKey] = user;

        // Continue with the action execution
        await next();
    }
}
