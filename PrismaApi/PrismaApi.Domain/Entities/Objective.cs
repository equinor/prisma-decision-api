using PrismaApi.Domain.Interfaces;
using System;

namespace PrismaApi.Domain.Entities;

public class Objective : AuditableEntity, IBaseEntity<Guid>
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public Project? Project { get; set; }
}
