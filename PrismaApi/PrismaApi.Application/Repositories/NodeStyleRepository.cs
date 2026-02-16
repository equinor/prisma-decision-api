using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Repositories;

public class NodeStyleRepository : BaseRepository<NodeStyle, System.Guid>
{
    public NodeStyleRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}
