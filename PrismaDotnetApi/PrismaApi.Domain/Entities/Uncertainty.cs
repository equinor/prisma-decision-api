using PrismaApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class Uncertainty : BaseEntity, IBaseEntity<Guid>
{
    public required Guid Id { get; set; }
    public required Guid IssueId { get; set; }
    public required Guid ProjectId { get; set; }
    public bool IsKey { get; set; } = true;
    public Issue? Issue { get; set; }
    public Project? Project { get; set; }
    public ICollection<Outcome> Outcomes { get; set; } = new List<Outcome>();
    public ICollection<DiscreteProbability> DiscreteProbabilities { get; set; } = new List<DiscreteProbability>();
}
