using System.Text.Json.Serialization;

namespace PrismaApi.Domain.Dtos;

public class PolicyTableOutgoingDto
{
    [JsonPropertyName("decision_id")]
    public required Guid DecisionId { get; set; }

    [JsonPropertyName("parent_state_ids")]
    public List<Guid> ParentStateIds { get; set; } = [];

    [JsonPropertyName("option_id")]
    public required Guid OptionId { get; set; }

    [JsonPropertyName("value")]
    public double Value { get; set; }
}
public class PolicyTableFromFastApiDto
{
    [JsonPropertyName("decision_id")]
    public required string DecisionId { get; set; }

    [JsonPropertyName("parent_state_ids")]
    public List<string> States { get; set; } = [];

    [JsonPropertyName("option_id")]
    public required string OptionId { get; set; }

    [JsonPropertyName("value")]
    public double Value { get; set; }
}
