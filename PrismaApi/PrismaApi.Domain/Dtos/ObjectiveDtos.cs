using System;

namespace PrismaApi.Domain.Dtos;

public class ObjectiveDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class ObjectiveViaProjectDto : ObjectiveDto
{
    public string Type { get; set; } = "Fundamental";
}

public class ObjectiveIncomingDto : ObjectiveDto
{
    public Guid ProjectId { get; set; }
    public string Type { get; set; } = "Fundamental";
}

public class ObjectiveOutgoingDto : ObjectiveDto
{
    public Guid ProjectId { get; set; }
    public string Type { get; set; } = "Fundamental";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
