using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PrismaApi.Domain.Dtos;

public class DiscreteProbabilityDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();
    [JsonPropertyName("outcome_id")]
    public Guid OutcomeId { get; set; }
    [JsonPropertyName("uncertainty_id")]
    public Guid UncertaintyId { get; set; }
    [JsonPropertyName("probability")]
    public double? Probability { get; set; }
    [JsonPropertyName("parent_outcome_ids")]
    public List<Guid> ParentOutcomeIds { get; set; } = new();
    //public List<DiscreteProbabilityParentOutcomeDto> ParentOutcomes { get; set; } = new();
    [JsonPropertyName("parent_option_ids")]
    public List<Guid> ParentOptionIds { get; set; } = new();
    //public List<DiscreteProbabilityParentOptionDto> ParentOptions { get; set; } = new();
}

public class DiscreteProbabilityParentOutcomeDto
{
    [JsonPropertyName("discrete_probability_id")]
    public Guid DiscreteProbabilityId { get; set; }
    [JsonPropertyName("parent_outcome_id")]
    public Guid ParentOutcomeId { get; set; }
}

public class DiscreteProbabilityParentOptionDto
{
    [JsonPropertyName("discrete_probability_id")]
    public Guid DiscreteProbabilityId { get; set; }
    [JsonPropertyName("parent_option_id")]
    public Guid ParentOptionId { get; set; }
}

public class DiscreteUtilityDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();
    [JsonPropertyName("value_metric_id")]
    public Guid ValueMetricId { get; set; }
    [JsonPropertyName("utility_id")]
    public Guid UtilityId { get; set; }
    [JsonPropertyName("utility_value")]
    public double? UtilityValue { get; set; }
    [JsonPropertyName("parent_outcome_ids")]
    public List<Guid> ParentOutcomeIds { get; set; } = new();
    //public List<DiscreteUtilityParentOutcomeDto> ParentOutcomes { get; set; } = new();
    [JsonPropertyName("parent_option_ids")]
    public List<Guid> ParentOptionIds { get; set; } = new();
    //public List<DiscreteUtilityParentOptionDto> ParentOptions { get; set; } = new();
}

public class DiscreteUtilityParentOutcomeDto
{
    [JsonPropertyName("discrete_utility_id")]
    public Guid DiscreteUtilityId { get; set; }
    [JsonPropertyName("parent_outcome_id")]
    public Guid ParentOutcomeId { get; set; }
}

public class DiscreteUtilityParentOptionDto
{
    [JsonPropertyName("discrete_utility_id")]
    public Guid DiscreteUtilityId { get; set; }
    [JsonPropertyName("parent_option_id")]
    public Guid ParentOptionId { get; set; }
}
