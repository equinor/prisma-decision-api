using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PrismaApi.Domain.Dtos;

public class DecisionDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();
    [JsonPropertyName("issue_id")]
    public Guid IssueId { get; set; }
}

public class DecisionIncomingDto : DecisionDto
{
    [JsonPropertyName("options")]
    public List<OptionIncomingDto> Options { get; set; } = new();
    [JsonPropertyName("type")]
    public string Type { get; set; } = "Focus";
}

public class DecisionOutgoingDto : DecisionDto
{
    [JsonPropertyName("options")]
    public List<OptionOutgoingDto> Options { get; set; } = new();
    [JsonPropertyName("type")]
    public string Type { get; set; } = "Focus";
}
