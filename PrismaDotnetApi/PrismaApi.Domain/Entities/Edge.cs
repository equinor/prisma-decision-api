using PrismaApi.Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class Edge : BaseEntity, IBaseEntity<Guid>
{
    public required Guid Id { get; set; }
    public required Guid TailId { get; set; }
    public required Guid HeadId { get; set; }
    public required Guid ProjectId { get; set; }

    public Project? Project { get; set; }
    public Node? TailNode { get; set; }
    public Node? HeadNode { get; set; }
}
