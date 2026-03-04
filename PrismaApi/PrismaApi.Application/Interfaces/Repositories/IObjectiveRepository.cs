using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IObjectiveRepository : ICrudRepository<Objective, Guid>
{
}
