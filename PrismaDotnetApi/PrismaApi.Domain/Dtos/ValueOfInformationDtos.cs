using System.Text.Json.Serialization;

namespace PrismaApi.Domain.Dtos;

public class ValueOfPerfectInformationRequestDto
{
    [JsonPropertyName("evidence")]
    public List<Guid> Evidence { get; set; } = [];
}

/// <summary>
/// Represents the expected utilities and value of perfect information for an uncertainty.
/// </summary>
public class ValueOfPerfectInformationDto
{
    [JsonPropertyName("uncertainty_issue_id")]
    public required Guid UncertaintyIssueId { get; set; }

    [JsonPropertyName("base_meu")]
    public required double BaseMeu { get; set; }

    [JsonPropertyName("meu_with_information")]
    public required double MeuWithInformation { get; set; }

    [JsonPropertyName("value_of_information")]
    public required double ValueOfInformation { get; set; }
}
