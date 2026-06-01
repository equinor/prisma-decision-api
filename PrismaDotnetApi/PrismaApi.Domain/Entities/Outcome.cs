using PrismaApi.Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class Outcome : BaseEntity, IBaseEntity<Guid>
{
    public required Guid Id { get; set; }
    public required Guid UncertaintyId { get; set; }
    public required Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Utility { get; set; }

    public Uncertainty? Uncertainty { get; set; }
    public Project? Project { get; set; }
}
