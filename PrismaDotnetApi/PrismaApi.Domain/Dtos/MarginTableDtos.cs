using System.Text.Json.Serialization;

namespace PrismaApi.Domain.Dtos;

public class MarginTableRowDto
{
    [JsonPropertyName("uncertainty_id")]
    public required Guid UncertaintyId { get; set; }

    [JsonPropertyName("outcome_id")]
    public required Guid OutcomeId { get; set; }

    [JsonPropertyName("probability")]
    public double Probability { get; set; }
    [JsonPropertyName("options_evidence")]
    public required List<Guid> OptionsEvidence { get; set; }
}