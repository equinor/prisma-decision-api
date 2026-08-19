using System.Text.Json.Serialization;

namespace PrismaApi.Domain.Dtos;

public class PolicyTableStatesOutgoingDto
{
    [JsonPropertyName("states")]
    public List<string> States { get; set; } = [];

    [JsonPropertyName("value")]
    public double Value { get; set; }
}

public class PolicyTableOutgoingDto
{
    [JsonPropertyName("decision_id")]
    public string DecisionId { get; set; } = string.Empty;

    [JsonPropertyName("rows")]
    public List<PolicyTableStatesOutgoingDto> Rows { get; set; } = [];
}
