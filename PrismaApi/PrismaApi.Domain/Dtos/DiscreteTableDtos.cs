using System;
using System.Collections.Generic;

namespace PrismaApi.Domain.Dtos;

public class DiscreteProbabilityDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OutcomeId { get; set; }
    public Guid UncertaintyId { get; set; }
    public double? Probability { get; set; }
    public List<DiscreteProbabilityParentOutcomeDto> ParentOutcomes { get; set; } = new();
    public List<DiscreteProbabilityParentOptionDto> ParentOptions { get; set; } = new();
}

public class DiscreteProbabilityParentOutcomeDto
{
    public Guid DiscreteProbabilityId { get; set; }
    public Guid ParentOutcomeId { get; set; }
}

public class DiscreteProbabilityParentOptionDto
{
    public Guid DiscreteProbabilityId { get; set; }
    public Guid ParentOptionId { get; set; }
}

public class DiscreteUtilityDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ValueMetricId { get; set; }
    public Guid UtilityId { get; set; }
    public double? UtilityValue { get; set; }
    public List<DiscreteUtilityParentOutcomeDto> ParentOutcomes { get; set; } = new();
    public List<DiscreteUtilityParentOptionDto> ParentOptions { get; set; } = new();
}

public class DiscreteUtilityParentOutcomeDto
{
    public Guid DiscreteUtilityId { get; set; }
    public Guid ParentOutcomeId { get; set; }
}

public class DiscreteUtilityParentOptionDto
{
    public Guid DiscreteUtilityId { get; set; }
    public Guid ParentOptionId { get; set; }
}
