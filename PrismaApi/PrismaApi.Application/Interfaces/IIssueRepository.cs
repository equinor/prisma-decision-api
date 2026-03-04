using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Interfaces;

public interface IIssueRepository : ICrudRepository<Issue, Guid>
{
    Task<ICollection<Issue>> GetIssuesInInfluenceDiagram(Guid projectId);
}
