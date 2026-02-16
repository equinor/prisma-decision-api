using System;

namespace PrismaApi.Domain.Entities;

public class DiscreteUtilityParentOption
{
    public Guid DiscreteUtilityId { get; set; }
    public Guid ParentOptionId { get; set; }

    public DiscreteUtility? DiscreteUtility { get; set; }
    public Option? ParentOption { get; set; }
}
