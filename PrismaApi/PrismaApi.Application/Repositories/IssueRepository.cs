using System.Linq;
using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Repositories;

public class IssueRepository : BaseRepository<Issue, Guid>
{
    public IssueRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public override async Task UpdateRangeAsync(IEnumerable<Issue> incommingEntities)
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
            entity.Type = incomingEntity.Type;
            entity.Boundary = incomingEntity.Boundary;
            entity.Name = incomingEntity.Name;
            entity.Description = incomingEntity.Description;
            entity.Order = incomingEntity.Order;
            entity.UpdatedById = incomingEntity.UpdatedById;
        }

        await DbContext.SaveChangesAsync();
    }

    protected override IQueryable<Issue> Query()
    {
        return DbContext.Issues
            .Include(i => i.Node!)
                .ThenInclude(n => n.NodeStyle)
            .Include(i => i.Decision!)
                .ThenInclude(d => d.Options)
            .Include(i => i.Uncertainty!)
                .ThenInclude(u => u.Outcomes)
            //.Include(i => i.Uncertainty!)
            //    .ThenInclude(u => u.DiscreteProbabilities)
            //    .ThenInclude(d => d.ParentOptions)
            //.Include(i => i.Uncertainty!)
            //    .ThenInclude(u => u.DiscreteProbabilities)
            //    .ThenInclude(d => d.ParentOutcomes)
            .Include(i => i.Utility);
    }
}
