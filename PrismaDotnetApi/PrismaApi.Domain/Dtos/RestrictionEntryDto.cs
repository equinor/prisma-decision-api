using System.Text.Json.Serialization;

namespace PrismaApi.Domain.Dtos;

public class RestrictionEntryDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();
    [JsonPropertyName("project_id")]
    public Guid ProjectId { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("restriction_value")]
    public double RestrictionValue { get; set; }
    [JsonPropertyName("parent_state_id")]
    public Guid? ParentStateId { get; set; }
    [JsonPropertyName("is_parent_uncertainty")]
    public bool IsParentUncertainty { get; set; }
    [JsonPropertyName("child_state_id")]
    public Guid? ChildStateId { get; set; }
    [JsonPropertyName("is_child_uncertainty")]
    public bool IsChildUncertainty { get; set; }
    [JsonPropertyName("restriction_table_id")]
    public Guid RestrictionTableId { get; set; }
}

public class RestrictionEntryIncomingDto : RestrictionEntryDto
{
}

public class RestrictionEntryOutgoingDto : RestrictionEntryDto
{
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
    [JsonPropertyName("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }
}