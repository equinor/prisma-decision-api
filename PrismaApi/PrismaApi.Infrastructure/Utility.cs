using System;
using System.Collections.Generic;

namespace PrismaApi.Infrastructure;

public partial class Utility
{
    public Guid Id { get; set; }

    public Guid IssueId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public virtual ICollection<DiscreteUtility> DiscreteUtilities { get; set; } = new List<DiscreteUtility>();

    public virtual Issue Issue { get; set; } = null!;
}
