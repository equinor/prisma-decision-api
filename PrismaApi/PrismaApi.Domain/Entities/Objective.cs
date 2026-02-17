using PrismaApi.Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class Objective : AuditableEntity, IBaseEntity<Guid>
{
    [Column("id")]
    public Guid Id { get; set; }
    [Column("project_id")]
    public Guid ProjectId { get; set; }
    [Column("name")]
    public string Name { get; set; } = string.Empty;
    [Column("type")]
    public string Type { get; set; } = string.Empty;
    [Column("description")]
    public string Description { get; set; } = string.Empty;

    public Project? Project { get; set; }
}
