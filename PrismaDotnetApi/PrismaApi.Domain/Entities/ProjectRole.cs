using PrismaApi.Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class ProjectRole : AuditableEntity, IBaseEntity<Guid>
{
    public required Guid Id { get; set; }
    public required Guid ProjectId { get; set; }
    public required string UserId { get; set; }
    public string Role { get; set; } = string.Empty;

    public Project? Project { get; set; }
    public User? User { get; set; }
}
