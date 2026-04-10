using System;
using System.Text.Json.Serialization;

namespace PrismaApi.Domain.Dtos;

public class ValueMetricDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class ValueMetricIncomingDto : ValueMetricDto
{
}

public class ValueMetricOutgoingDto : ValueMetricDto
{
}
