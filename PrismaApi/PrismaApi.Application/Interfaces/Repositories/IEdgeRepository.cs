using PrismaApi.Domain.Entities;
using System.Linq.Expressions;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IEdgeRepository : ICrudRepository<Edge, Guid>
{
    Task<ICollection<Edge>> GetEdgesInInfluenceDiagram(Guid projectId, Expression<Func<Edge, bool>>? filterPredicate);
}
