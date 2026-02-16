using System;

namespace PrismaApi.Domain.Entities;

public class DiscreteProbabilityParentOption
{
    public Guid DiscreteProbabilityId { get; set; }
    public Guid ParentOptionId { get; set; }

    public DiscreteProbability? DiscreteProbability { get; set; }
    public Option? ParentOption { get; set; }
}
