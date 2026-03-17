using System;
using System.Collections.Generic;

namespace PrismaApi.Infrastructure;

public partial class DiscreteUtility
{
    public Guid Id { get; set; }

    public Guid ValueMetricId { get; set; }

    public Guid UtilityId { get; set; }

    public double? UtilityValue { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public virtual Utility Utility { get; set; } = null!;

    public virtual ValueMetric ValueMetric { get; set; } = null!;

    public virtual ICollection<Option> ParentOptions { get; set; } = new List<Option>();

    public virtual ICollection<Outcome> ParentOutcomes { get; set; } = new List<Outcome>();
}
