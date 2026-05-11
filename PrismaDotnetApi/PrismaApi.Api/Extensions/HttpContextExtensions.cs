using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Api.Extensions;

public static class HttpContextExtensions
{
    public static UserOutgoingDto GetLoadedUser(this HttpContext context)
    {
        context.Items.TryGetValue(AppConstants.CurrentUserKey, out var userDto);
        if (userDto is UserOutgoingDto dto)
        {
            return dto;
        }
        throw new InvalidOperationException("User not loaded");
    }
}
