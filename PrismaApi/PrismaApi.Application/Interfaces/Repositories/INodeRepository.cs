using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface INodeRepository : ICrudRepository<Node, Guid>
{
}
