using PrismaApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class Uncertainty : BaseEntity, IBaseEntity<Guid>
{
    [Column("id")]
    public Guid Id { get; set; }
    [Column("issue_id")]
    public Guid IssueId { get; set; }
    [Column("is_key")]
    public bool IsKey { get; set; } = true;

    public Issue? Issue { get; set; }
    public ICollection<Outcome> Outcomes { get; set; } = new List<Outcome>();
    public ICollection<DiscreteProbability> DiscreteProbabilities { get; set; } = new List<DiscreteProbability>();
}
