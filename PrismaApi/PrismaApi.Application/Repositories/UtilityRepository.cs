using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Repositories;

public class UtilityRepository : BaseRepository<Utility, Guid>
{
    public UtilityRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    protected override IQueryable<Utility> Query()
    {
        return DbContext.Utilities
            .Include(u => u.DiscreteUtilities)
                .ThenInclude(du => du.ParentOutcomes)
            .Include(u => u.DiscreteUtilities)
                .ThenInclude(du => du.ParentOptions);
    }
}
