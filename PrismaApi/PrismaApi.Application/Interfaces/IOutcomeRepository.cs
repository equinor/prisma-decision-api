using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Interfaces;

public interface IOutcomeRepository : ICrudRepository<Outcome, Guid>
{
}
