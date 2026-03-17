using System;
using System.Collections.Generic;

namespace PrismaApi.Infrastructure;

public partial class Node
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }

    public Guid IssueId { get; set; }

    public string Name { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public virtual ICollection<Edge> EdgeHeads { get; set; } = new List<Edge>();

    public virtual ICollection<Edge> EdgeTails { get; set; } = new List<Edge>();

    public virtual Issue Issue { get; set; } = null!;

    public virtual ICollection<NodeStyle> NodeStyles { get; set; } = new List<NodeStyle>();

    public virtual Project Project { get; set; } = null!;
}
