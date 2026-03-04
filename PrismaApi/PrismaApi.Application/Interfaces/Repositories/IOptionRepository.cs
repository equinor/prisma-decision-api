using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IOptionRepository : ICrudRepository<Option, Guid>
{
}
