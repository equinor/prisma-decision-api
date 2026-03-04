using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IUtilityRepository : ICrudRepository<Utility, Guid>
{
}
