using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Interfaces;

public interface IUncertaintyRepository : ICrudRepository<Uncertainty, Guid>
{
}
