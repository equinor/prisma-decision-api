using PrismaApi.Domain.Interfaces;
using System;

namespace PrismaApi.Domain.Entities;

public class Edge : BaseEntity, IBaseEntity<Guid>
{
    public Guid Id { get; set; }
    public Guid TailId { get; set; }
    public Guid HeadId { get; set; }
    public Guid ProjectId { get; set; }

    public Project? Project { get; set; }
    public Node? TailNode { get; set; }
    public Node? HeadNode { get; set; }
}
