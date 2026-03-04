using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Interfaces;

public interface IDiscreteProbabilityRepository : ICrudRepository<DiscreteProbability, Guid>
{
}
