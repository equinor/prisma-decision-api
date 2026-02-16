using System;
using System.Collections.Generic;

namespace PrismaApi.Domain.Entities;

public class Option : BaseEntity
{
    public Guid Id { get; set; }
    public Guid DecisionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Utility { get; set; }

    public Decision? Decision { get; set; }
    public ICollection<StrategyOption> StrategyOptions { get; set; } = new List<StrategyOption>();
}
