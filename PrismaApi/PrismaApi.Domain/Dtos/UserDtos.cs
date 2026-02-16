using System;
using System.Collections.Generic;

namespace PrismaApi.Domain.Dtos;

public class UserDto
{
    public string Name { get; set; } = string.Empty;
    public string AzureId { get; set; } = string.Empty;
}

public class UserIncomingDto : UserDto
{
    public Guid? Id { get; set; }
    public List<ProjectRoleIncomingDto> ProjectRoles { get; set; } = new();
}

public class UserOutgoingDto : UserDto
{
    public Guid Id { get; set; }
    public List<ProjectRoleOutgoingDto> ProjectRoles { get; set; } = new();
}
