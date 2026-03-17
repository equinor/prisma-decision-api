using System;
using System.Text.Json.Serialization;

namespace PrismaApi.Domain.Dtos;

public class ProjectRoleDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();
    [JsonPropertyName("user_id")]
    public required string UserId { get; set; }
    [JsonPropertyName("project_id")]
    public Guid ProjectId { get; set; }
}

public class ProjectRoleIncomingDto : ProjectRoleDto
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class ProjectRoleCreateDto : ProjectRoleDto
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class ProjectRoleOutgoingDto : ProjectRoleDto
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
