using System.Linq;
using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Repositories;

public class StrategyRepository : BaseRepository<Strategy, Guid>
{
    public StrategyRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public override async Task UpdateRangeAsync(IEnumerable<Strategy> incommingEntities)
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

            entity.ProjectId = incomingEntity.ProjectId;
            entity.Name = incomingEntity.Name;
            entity.Description = incomingEntity.Description;
            entity.Rationale = incomingEntity.Rationale;
            entity.UpdatedById = incomingEntity.UpdatedById;
            entity.StrategyOptions = entity.StrategyOptions.Update(incomingEntity.StrategyOptions);
        }

        await DbContext.SaveChangesAsync();
    }

    protected override IQueryable<Strategy> Query()
    {
        return DbContext.Strategies
            .Include(s => s.StrategyOptions)
                .ThenInclude(so => so.Option);
    }
}
