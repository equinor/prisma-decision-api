using System;

namespace PrismaApi.Domain.Dtos;

public class IssueDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
}

public class IssueIncomingDto : IssueDto
{
    public string Type { get; set; } = "Unassigned";
    public string Boundary { get; set; } = "out";
    public NodeIncomingDto? Node { get; set; }
    public DecisionIncomingDto? Decision { get; set; }
    public UncertaintyIncomingDto? Uncertainty { get; set; }
    public UtilityIncomingDto? Utility { get; set; }
}

public class IssueOutgoingDto : IssueDto
{
    public string Type { get; set; } = "Unassigned";
    public string Boundary { get; set; } = "out";
    public NodeViaIssueOutgoingDto Node { get; set; } = new();
    public DecisionOutgoingDto? Decision { get; set; }
    public UncertaintyOutgoingDto? Uncertainty { get; set; }
    public UtilityOutgoingDto? Utility { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class IssueViaNodeOutgoingDto : IssueDto
{
    public string Type { get; set; } = "Unassigned";
    public string Boundary { get; set; } = "out";
    public DecisionOutgoingDto? Decision { get; set; }
    public UncertaintyOutgoingDto? Uncertainty { get; set; }
    public UtilityOutgoingDto? Utility { get; set; }
}
