using System.Text.Json.Serialization;

namespace PrismaApi.Domain.Dtos;

public class PolicyTableRowOutgoingDto
{
    [JsonPropertyName("states")]
    public Dictionary<string, string> States { get; set; } = [];

    [JsonPropertyName("value")]
    public double Value { get; set; }
}

public class PolicyTableDecisionOutgoingDto
{
    [JsonPropertyName("decision_id")]
    public string DecisionId { get; set; } = string.Empty;

    [JsonPropertyName("rows")]
    public List<PolicyTableRowOutgoingDto> Rows { get; set; } = [];
}
