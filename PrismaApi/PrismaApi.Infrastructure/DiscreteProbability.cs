using System;
using System.Collections.Generic;

namespace PrismaApi.Infrastructure;

public partial class DiscreteProbability
{
    public Guid Id { get; set; }

    public Guid OutcomeId { get; set; }

    public Guid UncertaintyId { get; set; }

    public double? Probability { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public virtual Outcome Outcome { get; set; } = null!;

    public virtual Uncertainty Uncertainty { get; set; } = null!;

    public virtual ICollection<Option> ParentOptions { get; set; } = new List<Option>();

    public virtual ICollection<Outcome> ParentOutcomes { get; set; } = new List<Outcome>();
}
