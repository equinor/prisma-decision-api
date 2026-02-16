using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Repositories;

public class OptionRepository : BaseRepository<Option, System.Guid>
{
    public OptionRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}
