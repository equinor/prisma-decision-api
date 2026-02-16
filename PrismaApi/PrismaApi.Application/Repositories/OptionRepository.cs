using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Repositories;

public class OptionRepository : BaseRepository<Option, Guid>
{
    public OptionRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}
