using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PrismaApi.Domain.Dtos;

public class UncertaintyDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();
    [JsonPropertyName("issue_id")]
    public Guid IssueId { get; set; }
    [JsonPropertyName("is_key")]
    public bool IsKey { get; set; } = true;
}

public class UncertaintyIncomingDto : UncertaintyDto
{
    [JsonPropertyName("outcomes")]
    public List<OutcomeIncomingDto> Outcomes { get; set; } = new();
    public List<DiscreteProbabilityDto> DiscreteProbabilities { get; set; } = new();
}

public class UncertaintyOutgoingDto : UncertaintyDto
{
    [JsonPropertyName("outcomes")]
    public List<OutcomeOutgoingDto> Outcomes { get; set; } = new();
    [JsonPropertyName("discrete_probabilities")]
    public List<DiscreteProbabilityDto> DiscreteProbabilities { get; set; } = new();
}
