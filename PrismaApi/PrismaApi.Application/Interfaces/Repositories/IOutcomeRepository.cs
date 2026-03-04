using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IOutcomeRepository : ICrudRepository<Outcome, Guid>
{
}
