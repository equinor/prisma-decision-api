using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PrismaApi.Domain.Dtos;

/// <summary>Full project payload sent to Python for pure duplication-logic computation.</summary>
public class FullProjectForDuplicationDto : ProjectOutgoingDto
{
    [JsonPropertyName("strategies")]
    public List<StrategyOutgoingDto> strategies { get; set; } = new();
    [JsonPropertyName("objectives")]
    public List<ObjectiveOutgoingDto> objectives { get; set; } = new();
    [JsonPropertyName("issues")]
    public List<IssueOutgoingDto> Issues { get; set; } = new();
    [JsonPropertyName("edges")]
    public List<EdgeOutgoingDto> Edges { get; set; } = new();
    [JsonPropertyName("assessments")]
    public List<AssessmentOutgoingDto> Assessments { get; set; } = new();
}
