using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PrismaApi.Domain.Dtos;

public class UserDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("azure_id")]
    public string AzureId { get; set; } = string.Empty;
}

public class UserIncomingDto : UserDto
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }
    [JsonPropertyName("project_roles")]
    public List<ProjectRoleIncomingDto> ProjectRoles { get; set; } = new();
}

public class UserOutgoingDto : UserDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("project_roles")]
    public List<ProjectRoleOutgoingDto> ProjectRoles { get; set; } = new();
}
