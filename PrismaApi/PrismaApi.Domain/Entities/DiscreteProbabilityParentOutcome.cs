using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class DiscreteProbabilityParentOutcome
{
    [Column("discrete_probability_id")]
    public Guid DiscreteProbabilityId { get; set; }
    [Column("parent_outcome_id")]
    public Guid ParentOutcomeId { get; set; }

    public DiscreteProbability? DiscreteProbability { get; set; }
    public Outcome? ParentOutcome { get; set; }
}
