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

    public override async Task UpdateRangeAsync(IEnumerable<Decision> incommingEntities)
    {
        var incomingList = incommingEntities.ToList();
        if (incomingList.Count == 0)
        {
            return;
        }

        var entities = await GetByIdsAsync(incomingList.Select(e => e.Id));
        foreach (var entity in entities)
        {
            var incomingEntity = incomingList.FirstOrDefault(x => x.Id == entity.Id);
            if (incomingEntity == null)
            {
                continue;
            }

            entity.IssueId = incomingEntity.IssueId;
            entity.Type = incomingEntity.Type;
            entity.Options.Update(incomingEntity.Options, DbContext);
        }

        await DbContext.SaveChangesAsync();
    }

    protected override IQueryable<Decision> Query()
    {
        return DbContext.Decisions
            .Include(d => d.Options);
    }
}
