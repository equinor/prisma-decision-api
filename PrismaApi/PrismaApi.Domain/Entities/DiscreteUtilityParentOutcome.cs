using System;

namespace PrismaApi.Domain.Entities;

public class DiscreteUtilityParentOutcome
{
    public Guid DiscreteUtilityId { get; set; }
    public Guid ParentOutcomeId { get; set; }

    public DiscreteUtility? DiscreteUtility { get; set; }
    public Outcome? ParentOutcome { get; set; }
}
