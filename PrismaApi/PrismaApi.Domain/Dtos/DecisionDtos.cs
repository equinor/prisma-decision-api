using System;
using System.Collections.Generic;

namespace PrismaApi.Domain.Dtos;

public class DecisionDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid IssueId { get; set; }
}

public class DecisionIncomingDto : DecisionDto
{
    public List<OptionIncomingDto> Options { get; set; } = new();
    public string Type { get; set; } = "Focus";
}

public class DecisionOutgoingDto : DecisionDto
{
    public List<OptionOutgoingDto> Options { get; set; } = new();
    public string Type { get; set; } = "Focus";
}
