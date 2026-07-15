using PrismaApi.Domain.Constants;

namespace PrismaApi.Domain.Extensions;

public static class ProjectRoleTypeExtensions
{
    public static bool IsFacilitator(this string role) => 
        string.Equals(role, ProjectRoleType.Facilitator.ToString(), StringComparison.OrdinalIgnoreCase);
}