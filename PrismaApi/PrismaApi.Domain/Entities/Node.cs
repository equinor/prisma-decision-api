using PrismaApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class Node : BaseEntity, IBaseEntity<Guid>
{
    [Column("id")]
    public Guid Id { get; set; }
    [Column("project_id")]
    public Guid ProjectId { get; set; }
    [Column("issue_id")]
    public Guid IssueId { get; set; }
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    public Project? Project { get; set; }
    public Issue? Issue { get; set; }

    public ICollection<Edge> HeadEdges { get; set; } = new List<Edge>();
    public ICollection<Edge> TailEdges { get; set; } = new List<Edge>();

    public NodeStyle? NodeStyle { get; set; }
}
