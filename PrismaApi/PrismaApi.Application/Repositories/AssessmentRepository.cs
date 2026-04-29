using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Context;

namespace PrismaApi.Application.Repositories
{
    public class AssessmentRepository : BaseRepository<Assessment, Guid>, IAssessmentRepository
    {
        public AssessmentRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        protected override IQueryable<Assessment> Query()
        {
            return DbContext.Assessments.Include(a => a.DecisionQualityAssessments);
        }

        public async Task UpdateAsync(List<Assessment> incommingEntities, Expression<Func<Assessment, bool>> filterPredicate, CancellationToken ct)
        {
            var incomingList = incommingEntities.ToList();
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
