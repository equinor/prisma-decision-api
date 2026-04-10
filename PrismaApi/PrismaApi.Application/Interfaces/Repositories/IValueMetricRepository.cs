using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IValueMetricRepository : ICrudRepository<ValueMetric, Guid>
{
}
