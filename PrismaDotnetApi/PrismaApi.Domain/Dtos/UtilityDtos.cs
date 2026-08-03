using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PrismaApi.Domain.Dtos;

public class UtilityDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();
    [JsonPropertyName("issue_id")]
    public Guid IssueId { get; set; }
    [JsonPropertyName("project_id")]
    public required Guid ProjectId { get; set; }
}

public class UtilityIncomingDto : UtilityDto
{
}

public class UtilityOutgoingDto : UtilityDto
{
}
