using System.Text.Json.Serialization;

namespace PrismaApi.Domain.Dtos;

public class EvidenceRequestDto
{
    [JsonPropertyName("evidence_id")]
    public Guid EvidenceId { get; set; }
    [JsonPropertyName("state_ids")]
    public List<Guid> StateIds { get; set; } = [];
}