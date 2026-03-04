using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Interfaces;

public interface IOptionRepository : ICrudRepository<Option, Guid>
{
}
