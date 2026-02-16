using System;

namespace PrismaApi.Domain.Entities;

public class ValueMetric : BaseEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
