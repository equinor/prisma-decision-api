using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Repositories;

public class ObjectiveRepository : BaseRepository<Objective, Guid>
{
    public ObjectiveRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}
