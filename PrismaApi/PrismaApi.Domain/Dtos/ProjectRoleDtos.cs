using System;

namespace PrismaApi.Domain.Dtos;

public class UserInfoDto
{
    public string UserName { get; set; } = string.Empty;
    public string AzureId { get; set; } = string.Empty;
}

public class ProjectRoleDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid ProjectId { get; set; }
}

public class ProjectRoleIncomingDto : ProjectRoleDto
{
    public string Role { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string AzureId { get; set; } = string.Empty;
}

public class ProjectRoleCreateDto : ProjectRoleDto
{
    public string Role { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string AzureId { get; set; } = string.Empty;
}

public class ProjectRoleOutgoingDto : ProjectRoleDto
{
    public string Role { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string AzureId { get; set; } = string.Empty;
}
