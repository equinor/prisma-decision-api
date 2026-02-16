using System;

namespace PrismaApi.Domain.Dtos;

public class OptionDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public Guid DecisionId { get; set; }
    public double Utility { get; set; }
}

public class OptionIncomingDto : OptionDto
{
}

public class OptionOutgoingDto : OptionDto
{
}
