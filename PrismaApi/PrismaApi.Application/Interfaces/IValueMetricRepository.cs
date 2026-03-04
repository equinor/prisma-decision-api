using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Interfaces;

public interface IValueMetricRepository : ICrudRepository<ValueMetric, Guid>
{
}
