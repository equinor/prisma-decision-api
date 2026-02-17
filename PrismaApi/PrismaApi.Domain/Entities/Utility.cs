using PrismaApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class Utility : BaseEntity, IBaseEntity<Guid>
{
    [Column("id")]
    public Guid Id { get; set; }
    [Column("issue_id")]
    public Guid IssueId { get; set; }

    public Issue? Issue { get; set; }
    public ICollection<DiscreteUtility> DiscreteUtilities { get; set; } = new List<DiscreteUtility>();
}
