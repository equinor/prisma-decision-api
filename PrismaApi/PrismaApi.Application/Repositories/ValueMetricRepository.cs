using System.Linq;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Context;

namespace PrismaApi.Application.Repositories;

public class ValueMetricRepository : BaseRepository<ValueMetric, Guid>, IValueMetricRepository
{
    public ValueMetricRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public override async Task UpdateRangeAsync(IEnumerable<ValueMetric> incommingEntities, CancellationToken ct = default)
    {
        var incomingList = incommingEntities.ToList();
        if (incomingList.Count == 0)
        {
            return;
        }

        var entities = await GetByIdsAsync(incomingList.Select(e => e.Id), ct: ct);
        foreach (var entity in entities)
        {
            var incomingEntity = incomingList.FirstOrDefault(x => x.Id == entity.Id);
            if (incomingEntity == null)
            {
                continue;
            }

            entity.Name = incomingEntity.Name;
        }

        await DbContext.SaveChangesAsync(ct);
    }
}
