using System;

namespace PrismaApi.Domain.Dtos;

public class NodeStyleDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid NodeId { get; set; }
    public double XPosition { get; set; }
    public double YPosition { get; set; }
}

public class NodeStyleIncomingDto : NodeStyleDto
{
}

public class NodeStyleOutgoingDto : NodeStyleDto
{
}
