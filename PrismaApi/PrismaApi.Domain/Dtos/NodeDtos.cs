using System;

namespace PrismaApi.Domain.Dtos;

public class NodeDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public Guid IssueId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class NodeIncomingDto : NodeDto
{
    public NodeStyleIncomingDto? NodeStyle { get; set; }
}

public class NodeOutgoingDto : NodeDto
{
    public IssueViaNodeOutgoingDto Issue { get; set; } = new();
    public NodeStyleOutgoingDto NodeStyle { get; set; } = new();
}

public class NodeViaIssueOutgoingDto : NodeDto
{
    public NodeStyleOutgoingDto NodeStyle { get; set; } = new();
}
