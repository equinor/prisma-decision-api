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
    [System.Text.Json.Serialization.JsonPropertyName("states")]
    public List<string> States { get; set; } = [];

    [System.Text.Json.Serialization.JsonPropertyName("option_id")]
    public required string OptionId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("value")]
    public double Value { get; set; }
}
