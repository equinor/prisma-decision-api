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
    [JsonPropertyName("type")]
    public string Type { get; set; } = "Fundamental";
    [JsonPropertyName("ordering")]
    public int Ordering { get; set; } = 0;
}

public class ObjectiveViaProjectDto : ObjectiveDto
{
}

public class ObjectiveIncomingDto : ObjectiveDto
{
    [JsonPropertyName("project_id")]
    public Guid ProjectId { get; set; }

}

public class ObjectiveOutgoingDto : ObjectiveDto
{
    [JsonPropertyName("project_id")]
    public Guid ProjectId { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
    [JsonPropertyName("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }
}
