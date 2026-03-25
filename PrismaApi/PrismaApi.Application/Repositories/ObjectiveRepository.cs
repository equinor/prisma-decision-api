using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Context;
using System.Linq;
using System.Linq.Expressions;

namespace PrismaApi.Application.Repositories;

public class ObjectiveRepository : BaseRepository<Objective, Guid>, IObjectiveRepository
{
    public ObjectiveRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task UpdateRangeAsync(IEnumerable<Objective> incommingEntities, Expression<Func<Objective, bool>> filterPredicate, CancellationToken ct = default)
    {
        var incomingList = incommingEntities.ToList();
        if (incomingList.Count == 0)
        {
            return;
        }

        var entities = await GetByIdsAsync(incomingList.Select(e => e.Id), filterPredicate: filterPredicate, ct: ct);
        foreach (var entity in entities)
        {
            var incomingEntity = incomingList.FirstOrDefault(x => x.Id == entity.Id);
            if (incomingEntity == null)
            {
                continue;
            }

            entity.ProjectId = incomingEntity.ProjectId;
            entity.Name = incomingEntity.Name;
            entity.Type = incomingEntity.Type;
            entity.Description = incomingEntity.Description;
            entity.UpdatedById = incomingEntity.UpdatedById;
        }

        await DbContext.SaveChangesAsync(ct);
    }
}
