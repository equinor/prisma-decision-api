using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class DiscreteProbabilityParentOutcome
{
    public required Guid DiscreteProbabilityId { get; set; }
    public required Guid ParentOutcomeId { get; set; }

    public DiscreteProbability? DiscreteProbability { get; set; }
    public Outcome? ParentOutcome { get; set; }
}
