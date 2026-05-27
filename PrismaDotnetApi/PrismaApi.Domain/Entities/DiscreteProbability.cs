using PrismaApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class DiscreteProbability : BaseEntity, IBaseEntity<Guid>
{
    public required Guid Id { get; set; }
    public required Guid OutcomeId { get; set; }
    public required Guid UncertaintyId { get; set; }
    public required Guid ProjectId { get; set; }
    public double? Probability { get; set; }

    public Outcome? Outcome { get; set; }
    public Uncertainty? Uncertainty { get; set; }
    public Project? Project { get; set; }

    public ICollection<DiscreteProbabilityParentOutcome> ParentOutcomes { get; set; } = new List<DiscreteProbabilityParentOutcome>();
    public ICollection<DiscreteProbabilityParentOption> ParentOptions { get; set; } = new List<DiscreteProbabilityParentOption>();
}
