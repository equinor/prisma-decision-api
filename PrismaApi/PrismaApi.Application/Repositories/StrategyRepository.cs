using System.Linq;
using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Repositories;

public class StrategyRepository : BaseRepository<Strategy, System.Guid>
{
    public StrategyRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    protected override IQueryable<Strategy> Query()
    {
        return DbContext.Strategies
            .Include(s => s.StrategyOptions)
                .ThenInclude(so => so.Option);
    }
}
