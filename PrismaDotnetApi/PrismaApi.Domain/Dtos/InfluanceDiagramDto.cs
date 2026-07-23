namespace PrismaApi.Domain.Dtos;

public class InfluenceDiagramDto
{
    public required Guid projectId { get; init; }
    public required ICollection<IssueOutgoingDto> issues { get; init; }
    public required ICollection<EdgeOutgoingDto> edges { get; init; }
    public required ICollection<DiscreteProbabilityDto> discreteProbabilities { get; init; }
    public required ICollection<RestrictionTableOutgoingDto> restrictionTables { get; init; }
}
