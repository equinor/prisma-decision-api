using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IProjectRepository : ICrudRepository<Project, Guid>
{
}
