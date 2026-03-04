using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IProjectRepository : ICrudRepository<Project, Guid>
{
    Task<List<Project>> GetAllAsyncProjects(UserOutgoingDto user, bool withTracking = true);
}
