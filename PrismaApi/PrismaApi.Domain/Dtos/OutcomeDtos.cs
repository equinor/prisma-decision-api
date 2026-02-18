using System;
using System.Text.Json.Serialization;

namespace PrismaApi.Domain.Dtos;

public class OutcomeDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("uncertainty_id")]
    public Guid UncertaintyId { get; set; }
    [JsonPropertyName("utility")]
    public double Utility { get; set; }
}

public class OutcomeIncomingDto : OutcomeDto
{
}

public class OutcomeOutgoingDto : OutcomeDto
{
}
