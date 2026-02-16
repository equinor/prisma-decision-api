using PrismaApi.Domain.Interfaces;
using System;

namespace PrismaApi.Domain.Entities;

public class ProjectRole : AuditableEntity, IBaseEntity<Guid>
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public int UserId { get; set; }
    public string Role { get; set; } = string.Empty;

    public Project? Project { get; set; }
    public User? User { get; set; }
}
