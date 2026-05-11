using System;
using System.Text.Json.Serialization;

namespace PrismaApi.Domain.Dtos;

public class NodeDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();
    [JsonPropertyName("project_id")]
    public Guid ProjectId { get; set; }
    [JsonPropertyName("issue_id")]
    public Guid IssueId { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class NodeIncomingDto : NodeDto
{
    [JsonPropertyName("node_style")]
    public NodeStyleIncomingDto? NodeStyle { get; set; }
}

public class NodeOutgoingDto : NodeDto
{
    [JsonPropertyName("issue")]
    public IssueViaNodeOutgoingDto Issue { get; set; } = new();
    [JsonPropertyName("node_style")]
    public NodeStyleOutgoingDto NodeStyle { get; set; } = new();
}

public class NodeViaIssueOutgoingDto : NodeDto
{
    [JsonPropertyName("node_style")]
    public NodeStyleOutgoingDto NodeStyle { get; set; } = new();
}
