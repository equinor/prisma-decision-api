using System.Text.Json.Serialization;

namespace PrismaApi.Domain.Dtos;
public class RestrictionTableDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();
    [JsonPropertyName("project_id")]
    public Guid ProjectId { get; set; }
    [JsonPropertyName("edge_id")]
    public Guid EdgeId { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;  
    
}

public class RestrictionTableIncomingDto : RestrictionTableDto
{
    [JsonPropertyName("restriction_entries")]
    public List<RestrictionEntryIncomingDto> RestrictionEntries { get; set; } = [];
}

public class RestrictionTableOutgoingDto : RestrictionTableDto
{
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
    [JsonPropertyName("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }
    [JsonPropertyName("restriction_entries")]
    public List<RestrictionEntryOutgoingDto> RestrictionEntries { get; set; } = [];
}