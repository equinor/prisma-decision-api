using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Interfaces;

public interface IUtilityRepository : ICrudRepository<Utility, Guid>
{
}
