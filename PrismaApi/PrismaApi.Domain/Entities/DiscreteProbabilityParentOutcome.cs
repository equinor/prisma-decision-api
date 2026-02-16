using System;

namespace PrismaApi.Domain.Entities;

public class DiscreteProbabilityParentOutcome
{
    public Guid DiscreteProbabilityId { get; set; }
    public Guid ParentOutcomeId { get; set; }

    public DiscreteProbability? DiscreteProbability { get; set; }
    public Outcome? ParentOutcome { get; set; }
}
