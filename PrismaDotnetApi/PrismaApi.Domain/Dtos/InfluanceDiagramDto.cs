namespace PrismaApi.Domain.Dtos;

public class InfluenceDiagramDto
{
    public required Guid projectId { get; init; }
    public required ICollection<IssueOutgoingDto> issues { get; init; }
    public required ICollection<EdgeOutgoingDto> edges { get; init; }
}
