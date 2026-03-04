using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Interfaces;

public interface IDiscreteUtilityRepository : ICrudRepository<DiscreteUtility, Guid>
{
}
