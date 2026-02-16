using System;
using System.Collections.Generic;

namespace PrismaApi.Domain.Dtos;

public class UtilityDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid IssueId { get; set; }
}

public class UtilityIncomingDto : UtilityDto
{
}

public class UtilityOutgoingDto : UtilityDto
{
    public List<DiscreteUtilityDto> DiscreteUtilities { get; set; } = new();
}
