using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Interfaces;

public interface IProjectRoleRepository : ICrudRepository<ProjectRole, Guid>
{
}
