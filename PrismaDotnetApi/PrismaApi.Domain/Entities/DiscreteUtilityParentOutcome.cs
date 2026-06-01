using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class DiscreteUtilityParentOutcome
{
    public required Guid DiscreteUtilityId { get; set; }
    public required Guid ParentOutcomeId { get; set; }

    public DiscreteUtility? DiscreteUtility { get; set; }
    public Outcome? ParentOutcome { get; set; }
}
