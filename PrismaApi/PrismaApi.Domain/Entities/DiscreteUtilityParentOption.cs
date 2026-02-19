using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class DiscreteUtilityParentOption
{
    [Column("discrete_utility_id")]
    public Guid DiscreteUtilityId { get; set; }
    [Column("parent_option_id")]
    public Guid ParentOptionId { get; set; }

    public DiscreteUtility? DiscreteUtility { get; set; }
    public Option? ParentOption { get; set; }
}
