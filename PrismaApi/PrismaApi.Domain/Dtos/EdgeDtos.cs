using System;

namespace PrismaApi.Domain.Dtos;

public class EdgeDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TailId { get; set; }
    public Guid HeadId { get; set; }
    public Guid ProjectId { get; set; }
}

public class EdgeIncomingDto : EdgeDto
{
}

public class EdgeOutgoingDto : EdgeDto
{
    public NodeOutgoingDto HeadNode { get; set; } = new();
    public NodeOutgoingDto TailNode { get; set; } = new();
    public Guid HeadIssueId { get; set; }
    public Guid TailIssueId { get; set; }
}
