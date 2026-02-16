using PrismaApi.Domain.Interfaces;
using System;
using System.Collections.Generic;

namespace PrismaApi.Domain.Entities;

public class DiscreteProbability : BaseEntity, IBaseEntity<Guid>
{
    public Guid Id { get; set; }
    public Guid OutcomeId { get; set; }
    public Guid UncertaintyId { get; set; }
    public double? Probability { get; set; }

    public Outcome? Outcome { get; set; }
    public Uncertainty? Uncertainty { get; set; }

    public ICollection<DiscreteProbabilityParentOutcome> ParentOutcomes { get; set; } = new List<DiscreteProbabilityParentOutcome>();
    public ICollection<DiscreteProbabilityParentOption> ParentOptions { get; set; } = new List<DiscreteProbabilityParentOption>();
}
