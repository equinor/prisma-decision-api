using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Interfaces;

public interface IEdgeRepository : ICrudRepository<Edge, Guid>
{
    Task<ICollection<Edge>> GetEdgesInInfluenceDiagram(Guid projectId);
}
