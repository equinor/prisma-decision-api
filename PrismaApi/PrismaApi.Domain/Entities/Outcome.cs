using System;

namespace PrismaApi.Domain.Entities;

public class Outcome : BaseEntity
{
    public Guid Id { get; set; }
    public Guid UncertaintyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Utility { get; set; }

    public Uncertainty? Uncertainty { get; set; }
}
