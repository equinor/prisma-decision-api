using System;
using System.Collections.Generic;

namespace PrismaApi.Infrastructure;

public partial class Uncertainty
{
    public Guid Id { get; set; }

    public Guid IssueId { get; set; }

    public bool IsKey { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public virtual ICollection<DiscreteProbability> DiscreteProbabilities { get; set; } = new List<DiscreteProbability>();

    public virtual Issue Issue { get; set; } = null!;

    public virtual ICollection<Outcome> Outcomes { get; set; } = new List<Outcome>();
}
