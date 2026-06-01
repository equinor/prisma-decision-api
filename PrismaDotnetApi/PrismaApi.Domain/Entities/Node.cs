using PrismaApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class Node : BaseEntity, IBaseEntity<Guid>
{
    public required Guid Id { get; set; }
    public required Guid ProjectId { get; set; }
    public required Guid IssueId { get; set; }
    public string Name { get; set; } = string.Empty;

    public Project? Project { get; set; }
    public Issue? Issue { get; set; }

    public ICollection<Edge> HeadEdges { get; set; } = new List<Edge>();
    public ICollection<Edge> TailEdges { get; set; } = new List<Edge>();
    public NodeStyle? NodeStyle { get; set; }
}
