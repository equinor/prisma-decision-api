using System;
using System.Collections.Generic;

namespace PrismaApi.Infrastructure;

public partial class Issue
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }

    public string Type { get; set; } = null!;

    public string Boundary { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int Order { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public int CreatedById { get; set; }

    public int UpdatedById { get; set; }

    public virtual User CreatedBy { get; set; } = null!;

    public virtual ICollection<Decision> Decisions { get; set; } = new List<Decision>();

    public virtual ICollection<Node> Nodes { get; set; } = new List<Node>();

    public virtual Project Project { get; set; } = null!;

    public virtual ICollection<Uncertainty> Uncertainties { get; set; } = new List<Uncertainty>();

    public virtual User UpdatedBy { get; set; } = null!;

    public virtual ICollection<Utility> Utilities { get; set; } = new List<Utility>();
}
