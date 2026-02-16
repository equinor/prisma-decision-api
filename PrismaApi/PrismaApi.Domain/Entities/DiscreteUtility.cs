using PrismaApi.Domain.Interfaces;
using System;
using System.Collections.Generic;

namespace PrismaApi.Domain.Entities;

public class DiscreteUtility : BaseEntity, IBaseEntity<Guid>
{
    public Guid Id { get; set; }
    public Guid ValueMetricId { get; set; }
    public Guid UtilityId { get; set; }
    public double? UtilityValue { get; set; }

    public ValueMetric? ValueMetric { get; set; }
    public Utility? Utility { get; set; }

    public ICollection<DiscreteUtilityParentOutcome> ParentOutcomes { get; set; } = new List<DiscreteUtilityParentOutcome>();
    public ICollection<DiscreteUtilityParentOption> ParentOptions { get; set; } = new List<DiscreteUtilityParentOption>();
}
