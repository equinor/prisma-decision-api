using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IIssueRepository : ICrudRepository<Issue, Guid>
{
    Task<ICollection<Issue>> GetIssuesInInfluenceDiagram(Guid projectId);
}
