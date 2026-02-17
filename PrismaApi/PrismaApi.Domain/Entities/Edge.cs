using PrismaApi.Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class Edge : BaseEntity, IBaseEntity<Guid>
{
    [Column("id")]
    public Guid Id { get; set; }
    [Column("tail_id")]
    public Guid TailId { get; set; }
    [Column("head_id")]
    public Guid HeadId { get; set; }
    [Column("project_id")]
    public Guid ProjectId { get; set; }

    public Project? Project { get; set; }
    public Node? TailNode { get; set; }
    public Node? HeadNode { get; set; }
}
