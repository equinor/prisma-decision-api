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

public abstract class UtilityWithDiscreteUtilitiesDto : UtilityDto
{
    [JsonPropertyName("discrete_utilities")]
    public List<DiscreteUtilityDto> DiscreteUtilities { get; set; } = new();
}

public class UtilityIncomingDto : UtilityWithDiscreteUtilitiesDto
{
}

public class UtilityOutgoingDto : UtilityWithDiscreteUtilitiesDto
{
}
