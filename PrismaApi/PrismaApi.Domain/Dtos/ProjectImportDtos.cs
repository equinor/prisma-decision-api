using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PrismaApi.Domain.Dtos;

/// <summary>DTO for importing projects with their related entities (issues, edges, strategies).</summary>
public class ProjectImportDto
{
    [JsonPropertyName("projects")]
    public ProjectIncomingDto Projects { get; set; } = new();

    [JsonPropertyName("issues")]
    public List<IssueIncomingDto> Issues { get; set; } = new();

    [JsonPropertyName("edges")]
    public List<EdgeIncomingDto> Edges { get; set; } = new();
}
