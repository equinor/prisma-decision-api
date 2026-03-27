using PrismaApi.Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class NodeStyle : BaseEntity, IBaseEntity<Guid>
{
    public Guid Id { get; set; }
    public Guid NodeId { get; set; }
    public double XPosition { get; set; }
    public double YPosition { get; set; }

    public Node? Node { get; set; }
}
