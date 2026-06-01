using PrismaApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class Utility : BaseEntity, IBaseEntity<Guid>
{
    public required Guid Id { get; set; }
    public required Guid IssueId { get; set; }
    public required Guid ProjectId { get; set; }
    public Issue? Issue { get; set; }
    public Project? Project { get; set; }
    public ICollection<DiscreteUtility> DiscreteUtilities { get; set; } = new List<DiscreteUtility>();
}
