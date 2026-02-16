using System;
using System.Collections.Generic;

namespace PrismaApi.Domain.Entities;

public class Strategy : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Rationale { get; set; } = string.Empty;

    public Project? Project { get; set; }
    public ICollection<StrategyOption> StrategyOptions { get; set; } = new List<StrategyOption>();
}
