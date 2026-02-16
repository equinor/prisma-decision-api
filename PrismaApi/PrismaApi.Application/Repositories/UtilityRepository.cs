using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Repositories;

public class UtilityRepository : BaseRepository<Utility, Guid>
{
    public UtilityRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}
