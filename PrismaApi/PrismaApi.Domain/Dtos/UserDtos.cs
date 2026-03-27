using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PrismaApi.Domain.Dtos;

public class UserDto
{
    [JsonPropertyName("user_id")]
    public required string Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class UserIncomingDto : UserDto
{
    [JsonPropertyName("project_roles")]
    public List<ProjectRoleIncomingDto> ProjectRoles { get; set; } = new();
}

public class UserOutgoingDto : UserDto
{

}
