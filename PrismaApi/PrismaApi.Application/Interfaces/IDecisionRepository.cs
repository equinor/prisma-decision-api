using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Interfaces;

public interface IDecisionRepository : ICrudRepository<Decision, Guid>
{
}
