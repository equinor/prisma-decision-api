using System;
using System.Collections.Generic;

namespace PrismaApi.Domain.Dtos;

public class UncertaintyDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid IssueId { get; set; }
    public bool IsKey { get; set; } = true;
}

public class UncertaintyIncomingDto : UncertaintyDto
{
    public List<OutcomeIncomingDto> Outcomes { get; set; } = new();
}

public class UncertaintyOutgoingDto : UncertaintyDto
{
    public List<OutcomeOutgoingDto> Outcomes { get; set; } = new();
}
