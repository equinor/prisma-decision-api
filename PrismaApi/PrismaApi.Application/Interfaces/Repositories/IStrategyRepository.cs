using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IStrategyRepository : ICrudRepository<Strategy, Guid>
{
}
