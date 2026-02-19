using PrismaApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class DiscreteUtility : BaseEntity, IBaseEntity<Guid>
{
    [Column("id")]
    public Guid Id { get; set; }
    [Column("value_metric_id")]
    public Guid ValueMetricId { get; set; }
    [Column("utility_id")]
    public Guid UtilityId { get; set; }
    [Column("utility_value")]
    public double? UtilityValue { get; set; }

    public ValueMetric? ValueMetric { get; set; }
    public Utility? Utility { get; set; }

    public ICollection<DiscreteUtilityParentOutcome> ParentOutcomes { get; set; } = new List<DiscreteUtilityParentOutcome>();
    public ICollection<DiscreteUtilityParentOption> ParentOptions { get; set; } = new List<DiscreteUtilityParentOption>();
}
