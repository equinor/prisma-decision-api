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
}

public class UtilityIncomingDto : UtilityDto
{
}

public class UtilityOutgoingDto : UtilityDto
{
    [JsonPropertyName("discrete_utilities")]
    public List<DiscreteUtilityDto> DiscreteUtilities { get; set; } = new();
}
