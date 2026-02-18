using System;
using System.Text.Json.Serialization;

namespace PrismaApi.Domain.Dtos;

public class IssueDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();
    [JsonPropertyName("project_id")]
    public Guid ProjectId { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    [JsonPropertyName("order")]
    public int Order { get; set; }
}

public class IssueIncomingDto : IssueDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "Unassigned";
    [JsonPropertyName("boundary")]
    public string Boundary { get; set; } = "out";
    [JsonPropertyName("node")]
    public NodeIncomingDto? Node { get; set; }
    [JsonPropertyName("decision")]
    public DecisionIncomingDto? Decision { get; set; }
    [JsonPropertyName("uncertainty")]
    public UncertaintyIncomingDto? Uncertainty { get; set; }
    [JsonPropertyName("utility")]
    public UtilityIncomingDto? Utility { get; set; }
}

public class IssueOutgoingDto : IssueDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "Unassigned";
    [JsonPropertyName("boundary")]
    public string Boundary { get; set; } = "out";
    [JsonPropertyName("node")]
    public NodeViaIssueOutgoingDto Node { get; set; } = new();
    [JsonPropertyName("decision")]
    public DecisionOutgoingDto? Decision { get; set; }
    [JsonPropertyName("uncertainty")]
    public UncertaintyOutgoingDto? Uncertainty { get; set; }
    [JsonPropertyName("utility")]
    public UtilityOutgoingDto? Utility { get; set; }
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
    [JsonPropertyName("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }
}

public class IssueViaNodeOutgoingDto : IssueDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "Unassigned";
    [JsonPropertyName("boundary")]
    public string Boundary { get; set; } = "out";
    [JsonPropertyName("decision")]
    public DecisionOutgoingDto? Decision { get; set; }
    [JsonPropertyName("uncertainty")]
    public UncertaintyOutgoingDto? Uncertainty { get; set; }
    [JsonPropertyName("utility")]
    public UtilityOutgoingDto? Utility { get; set; }
}
