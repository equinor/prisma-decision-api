using System;
using System.Text.Json.Serialization;

namespace PrismaApi.Domain.Dtos;

public class ObjectiveDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

public class ObjectiveViaProjectDto : ObjectiveDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "Fundamental";
}

public class ObjectiveIncomingDto : ObjectiveDto
{
    [JsonPropertyName("project_id")]
    public Guid ProjectId { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; } = "Fundamental";
}

public class ObjectiveOutgoingDto : ObjectiveDto
{
    [JsonPropertyName("project_id")]
    public Guid ProjectId { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; } = "Fundamental";
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
    [JsonPropertyName("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }
}
