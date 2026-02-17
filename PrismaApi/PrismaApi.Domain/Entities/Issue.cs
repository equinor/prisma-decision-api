using PrismaApi.Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class Issue : AuditableEntity, IBaseEntity<Guid>
{
    [Column("id")]
    public Guid Id { get; set; }
    [Column("project_id")]
    public Guid ProjectId { get; set; }
    [Column("type")]
    public string Type { get; set; } = string.Empty;
    [Column("boundary")]
    public string Boundary { get; set; } = string.Empty;
    [Column("name")]
    public string Name { get; set; } = string.Empty;
    [Column("description")]
    public string Description { get; set; } = string.Empty;
    [Column("order")]
    public int Order { get; set; }

    public Project? Project { get; set; }
    public Node? Node { get; set; }
    public Decision? Decision { get; set; }
    public Uncertainty? Uncertainty { get; set; }
    public Utility? Utility { get; set; }
}
