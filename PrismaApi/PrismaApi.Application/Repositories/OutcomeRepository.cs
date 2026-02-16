using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Repositories;

public class OutcomeRepository : BaseRepository<Outcome, Guid>
{
    public OutcomeRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}
