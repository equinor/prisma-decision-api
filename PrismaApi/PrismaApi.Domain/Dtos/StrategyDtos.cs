using System;
using System.Collections.Generic;

namespace PrismaApi.Domain.Dtos;

public class StrategyDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Rationale { get; set; } = string.Empty;
}

public class StrategyIncomingDto : StrategyDto
{
    public Guid ProjectId { get; set; }
    public List<OptionIncomingDto> Options { get; set; } = new();
}

public class StrategyOutgoingDto : StrategyDto
{
    public Guid ProjectId { get; set; }
    public List<OptionOutgoingDto> Options { get; set; } = new();
}
