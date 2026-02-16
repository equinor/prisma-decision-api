using System;

namespace PrismaApi.Domain.Entities;

public class Issue : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Boundary { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }

    public Project? Project { get; set; }
    public Node? Node { get; set; }
    public Decision? Decision { get; set; }
    public Uncertainty? Uncertainty { get; set; }
    public Utility? Utility { get; set; }
}
