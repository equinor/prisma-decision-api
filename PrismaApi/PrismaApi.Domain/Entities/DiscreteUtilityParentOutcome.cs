using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class DiscreteUtilityParentOutcome
{
    [Column("discrete_utility_id")]
    public Guid DiscreteUtilityId { get; set; }
    [Column("parent_outcome_id")]
    public Guid ParentOutcomeId { get; set; }

    public DiscreteUtility? DiscreteUtility { get; set; }
    public Outcome? ParentOutcome { get; set; }
}
