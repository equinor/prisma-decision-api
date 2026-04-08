
using System.Linq.Expressions;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Context;

namespace PrismaApi.Application.Repositories
{
    public class SpiderAssessmentRepository : BaseRepository<SpiderAssessment, Guid>, ISpiderAssessmentRepository
    {

        public SpiderAssessmentRepository(AppDbContext dbContext) : base(dbContext)
        {

        }

        public async Task UpdateRangeAsync(List<SpiderAssessment> incomingEntities, Expression<Func<SpiderAssessment, bool>> filterPredicate, CancellationToken ct)
        {

            var incomingList = incomingEntities.ToList();
            if (incomingList.Count == 0)
            {
                return;
            }
            var entities = await GetByIdsAsync(incomingList.Select(e => e.Id), filterPredicate: filterPredicate);
            if (entities.Count != incomingList.Count)
                incomingList = incomingList.Where(e => entities.Select(x => x.Id).Contains(e.Id)).ToList();
            entities.Update(incomingList, DbContext);
            await DbContext.SaveChangesAsync();
        }
    }
}
