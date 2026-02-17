using PrismaApi.Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class ProjectRole : AuditableEntity, IBaseEntity<Guid>
{
    [Column("id")]
    public Guid Id { get; set; }
    [Column("project_id")]
    public Guid ProjectId { get; set; }
    [Column("user_id")]
    public int UserId { get; set; }
    [Column("role")]
    public string Role { get; set; } = string.Empty;

    public Project? Project { get; set; }
    public User? User { get; set; }
}
