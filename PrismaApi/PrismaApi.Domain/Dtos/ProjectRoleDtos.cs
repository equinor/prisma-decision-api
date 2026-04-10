using PrismaApi.Domain.Converters;
using System;
using System.Text.Json.Serialization;

namespace PrismaApi.Domain.Dtos;

public class ProjectRoleDto
{
    private string _userId = string.Empty;
    private bool _hasAzureId;

    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();
    [JsonPropertyName("user_id")]
    [JsonConverter(typeof(UserIdConverter))]
    public required string UserId
    {
        get => _userId;
        set
        {
            if (_hasAzureId)
            {
                return;
            }

            _userId = value;
        }
    }
    [JsonPropertyName("azure_id")]
    public string? AzureId
    {
        get => null;
        set
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                _hasAzureId = true;
                _userId = value;
            }
        }
    }
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
