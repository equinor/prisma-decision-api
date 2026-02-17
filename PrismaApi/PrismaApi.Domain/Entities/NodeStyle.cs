using PrismaApi.Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class NodeStyle : BaseEntity, IBaseEntity<Guid>
{
    [Column("id")]
    public Guid Id { get; set; }
    [Column("node_id")]
    public Guid NodeId { get; set; }
    [Column("x_position")]
    public double XPosition { get; set; }
    [Column("y_position")]
    public double YPosition { get; set; }

    public Node? Node { get; set; }
}
