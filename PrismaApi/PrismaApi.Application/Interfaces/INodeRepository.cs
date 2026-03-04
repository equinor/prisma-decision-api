using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Interfaces;

public interface INodeRepository : ICrudRepository<Node, Guid>
{
}
