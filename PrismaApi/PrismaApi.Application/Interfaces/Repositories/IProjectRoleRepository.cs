using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IProjectRoleRepository : ICrudRepository<ProjectRole, Guid>
{
}
