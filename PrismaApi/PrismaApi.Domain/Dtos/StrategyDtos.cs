using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PrismaApi.Domain.Dtos;

public class StrategyDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    [JsonPropertyName("rationale")]
    public string Rationale { get; set; } = string.Empty;
}

public class StrategyIncomingDto : StrategyDto
{
    [JsonPropertyName("project_id")]
    public Guid ProjectId { get; set; }
    [JsonPropertyName("options")]
    public List<OptionIncomingDto> Options { get; set; } = new();
}

public class StrategyOutgoingDto : StrategyDto
{
    [JsonPropertyName("project_id")]
    public Guid ProjectId { get; set; }
    [JsonPropertyName("options")]
    public List<OptionOutgoingDto> Options { get; set; } = new();
}
