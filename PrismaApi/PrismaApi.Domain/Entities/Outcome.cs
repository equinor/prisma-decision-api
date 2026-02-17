using PrismaApi.Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class Outcome : BaseEntity, IBaseEntity<Guid>
{
    [Column("id")]
    public Guid Id { get; set; }
    [Column("uncertainty_id")]
    public Guid UncertaintyId { get; set; }
    [Column("name")]
    public string Name { get; set; } = string.Empty;
    [Column("utility")]
    public double Utility { get; set; }

    public Uncertainty? Uncertainty { get; set; }
}
