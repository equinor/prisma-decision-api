using System;
using System.Text.Json.Serialization;

namespace PrismaApi.Domain.Dtos;

public class EdgeDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();
    [JsonPropertyName("tail_id")]
    public Guid TailId { get; set; }
    [JsonPropertyName("head_id")]
    public Guid HeadId { get; set; }
    [JsonPropertyName("project_id")]
    public Guid ProjectId { get; set; }
}

public class EdgeIncomingDto : EdgeDto
{
}

public class EdgeOutgoingDto : EdgeDto
{
    [JsonPropertyName("head_node")]
    public NodeOutgoingDto HeadNode { get; set; } = new();
    [JsonPropertyName("tail_node")]
    public NodeOutgoingDto TailNode { get; set; } = new();
    [JsonPropertyName("head_issue_id")]
    public Guid HeadIssueId { get; set; }
    [JsonPropertyName("tail_issue_id")]
    public Guid TailIssueId { get; set; }
}
