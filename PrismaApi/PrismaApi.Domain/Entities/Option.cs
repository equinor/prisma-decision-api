using PrismaApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class Option : BaseEntity, IBaseEntity<Guid>
{
    [Column("id")]
    public Guid Id { get; set; }
    [Column("decision_id")]
    public Guid DecisionId { get; set; }
    [Column("name")]
    public string Name { get; set; } = string.Empty;
    [Column("utility")]
    public double Utility { get; set; }

    public Decision? Decision { get; set; }
    public ICollection<StrategyOption> StrategyOptions { get; set; } = new List<StrategyOption>();
}
