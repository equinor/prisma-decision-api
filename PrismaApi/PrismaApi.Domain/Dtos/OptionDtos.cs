using System;
using System.Text.Json.Serialization;

namespace PrismaApi.Domain.Dtos;

public class OptionDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("decision_id")]
    public Guid DecisionId { get; set; }
    [JsonPropertyName("utility")]
    public double Utility { get; set; }
}

public class OptionIncomingDto : OptionDto
{
}

public class OptionOutgoingDto : OptionDto
{
}
