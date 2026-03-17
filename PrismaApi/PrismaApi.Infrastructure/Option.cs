using System;
using System.Collections.Generic;

namespace PrismaApi.Infrastructure;

public partial class Option
{
    public Guid Id { get; set; }

    public Guid DecisionId { get; set; }

    public string Name { get; set; } = null!;

    public double Utility { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public virtual Decision Decision { get; set; } = null!;

    public virtual ICollection<DiscreteProbability> DiscreteProbabilities { get; set; } = new List<DiscreteProbability>();

    public virtual ICollection<DiscreteUtility> DiscreteUtilities { get; set; } = new List<DiscreteUtility>();

    public virtual ICollection<Strategy> Strategies { get; set; } = new List<Strategy>();
}
