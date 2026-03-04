using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Interfaces;

public interface INodeStyleRepository : ICrudRepository<NodeStyle, Guid>
{
}
