using PrismaApi.Domain.Interfaces;
using System;
using System.Collections.Generic;

namespace PrismaApi.Domain.Entities;

public class Utility : BaseEntity, IBaseEntity<Guid>
{
    public Guid Id { get; set; }
    public Guid IssueId { get; set; }

    public Issue? Issue { get; set; }
    public ICollection<DiscreteUtility> DiscreteUtilities { get; set; } = new List<DiscreteUtility>();
}
