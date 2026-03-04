using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IDecisionRepository : ICrudRepository<Decision, Guid>
{
}
