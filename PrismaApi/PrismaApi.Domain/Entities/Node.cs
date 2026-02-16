using PrismaApi.Domain.Interfaces;
using System;
using System.Collections.Generic;

namespace PrismaApi.Domain.Entities;

public class Node : BaseEntity, IBaseEntity<Guid>
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid IssueId { get; set; }
    public string Name { get; set; } = string.Empty;

    public Project? Project { get; set; }
    public Issue? Issue { get; set; }

    public ICollection<Edge> HeadEdges { get; set; } = new List<Edge>();
    public ICollection<Edge> TailEdges { get; set; } = new List<Edge>();

    public NodeStyle? NodeStyle { get; set; }
}
