using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class DiscreteUtilityParentOption
{
    public required Guid DiscreteUtilityId { get; set; }
    public required Guid ParentOptionId { get; set; }

    public DiscreteUtility? DiscreteUtility { get; set; }
    public Option? ParentOption { get; set; }
}
