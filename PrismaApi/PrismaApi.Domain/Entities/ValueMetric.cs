using PrismaApi.Domain.Interfaces;
using System;

namespace PrismaApi.Domain.Entities;

public class ValueMetric : BaseEntity, IBaseEntity<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
