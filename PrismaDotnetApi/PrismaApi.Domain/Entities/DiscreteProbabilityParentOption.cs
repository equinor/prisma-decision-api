using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class DiscreteProbabilityParentOption
{
    public required Guid DiscreteProbabilityId { get; set; }
    public required Guid ParentOptionId { get; set; }

    public DiscreteProbability? DiscreteProbability { get; set; }
    public Option? ParentOption { get; set; }
}
