using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Repositories;

public class ValueMetricRepository : BaseRepository<ValueMetric, System.Guid>
{
    public ValueMetricRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}
