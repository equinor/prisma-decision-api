using PrismaApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class DiscreteProbability : BaseEntity, IBaseEntity<Guid>
{
    [Column("id")]
    public Guid Id { get; set; }
    [Column("outcome_id")]
    public Guid OutcomeId { get; set; }
    [Column("uncertainty_id")]
    public Guid UncertaintyId { get; set; }
    [Column("probability")]
    public double? Probability { get; set; }

    public Outcome? Outcome { get; set; }
    public Uncertainty? Uncertainty { get; set; }

    public ICollection<DiscreteProbabilityParentOutcome> ParentOutcomes { get; set; } = new List<DiscreteProbabilityParentOutcome>();
    public ICollection<DiscreteProbabilityParentOption> ParentOptions { get; set; } = new List<DiscreteProbabilityParentOption>();
}
