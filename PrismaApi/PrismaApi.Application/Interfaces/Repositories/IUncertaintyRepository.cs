using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IUncertaintyRepository : ICrudRepository<Uncertainty, Guid>
{
}
