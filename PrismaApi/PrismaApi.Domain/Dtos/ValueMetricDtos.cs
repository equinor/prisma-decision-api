using System;

namespace PrismaApi.Domain.Dtos;

public class ValueMetricDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
}

public class ValueMetricIncomingDto : ValueMetricDto
{
}

public class ValueMetricOutgoingDto : ValueMetricDto
{
}
