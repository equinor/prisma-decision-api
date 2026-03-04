using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Interfaces;

public interface IStrategyRepository : ICrudRepository<Strategy, Guid>
{
}
