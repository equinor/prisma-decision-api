using System.Linq;
using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Repositories;

public class DecisionRepository : BaseRepository<Decision, Guid>
{
    public DecisionRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    protected override IQueryable<Decision> Query()
    {
        return DbContext.Decisions
            .Include(d => d.Options);
    }
}
