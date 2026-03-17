using System;
using System.Collections.Generic;

namespace PrismaApi.Infrastructure;

public partial class ValueMetric
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public virtual ICollection<DiscreteUtility> DiscreteUtilities { get; set; } = new List<DiscreteUtility>();
}
