using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class DiscreteProbabilityParentOption
{
    [Column("discrete_probability_id")]
    public Guid DiscreteProbabilityId { get; set; }
    [Column("parent_option_id")]
    public Guid ParentOptionId { get; set; }

    public DiscreteProbability? DiscreteProbability { get; set; }
    public Option? ParentOption { get; set; }
}
