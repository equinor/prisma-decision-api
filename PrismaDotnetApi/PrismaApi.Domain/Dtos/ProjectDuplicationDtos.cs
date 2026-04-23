using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PrismaApi.Domain.Dtos;

/// <summary>Full project payload sent to Python for pure duplication-logic computation.</summary>
public class FullProjectForDuplicationDto : ProjectOutgoingDto
{
    [JsonPropertyName("issues")]
    public List<IssueOutgoingDto> Issues { get; set; } = new();
    [JsonPropertyName("edges")]
    public List<EdgeOutgoingDto> Edges { get; set; } = new();
}

/// <summary>Blueprint returned by Python holding all DTOs that .NET must persist.</summary>
public class ProjectDuplicateBlueprintDto
{
    [JsonPropertyName("project")]
    public ProjectCreateDto Project { get; set; } = new();
    [JsonPropertyName("issues")]
    public List<IssueIncomingDto> Issues { get; set; } = new();
    [JsonPropertyName("discrete_probabilities")]
    public List<DiscreteProbabilityDto> DiscreteProbabilities { get; set; } = new();
    [JsonPropertyName("discrete_utilities")]
    public List<DiscreteUtilityDto> DiscreteUtilities { get; set; } = new();
    [JsonPropertyName("strategies")]
    public List<StrategyIncomingDto> Strategies { get; set; } = new();
    [JsonPropertyName("edges")]
    public List<EdgeIncomingDto> Edges { get; set; } = new();
}
