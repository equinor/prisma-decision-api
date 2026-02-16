using System;

namespace PrismaApi.Domain.Dtos;

public class OutcomeDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public Guid UncertaintyId { get; set; }
    public double Utility { get; set; }
}

public class OutcomeIncomingDto : OutcomeDto
{
}

public class OutcomeOutgoingDto : OutcomeDto
{
}
