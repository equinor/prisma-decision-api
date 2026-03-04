using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Interfaces;

public interface IObjectiveRepository : ICrudRepository<Objective, Guid>
{
}
