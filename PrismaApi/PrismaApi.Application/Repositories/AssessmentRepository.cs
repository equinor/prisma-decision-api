using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Repositories
{
    public class AssessmentRepository : BaseRepository<Assessment, Guid>, IAssessmentRepository
    {
        public AssessmentRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task UpdateAsync(List<AssessmentIncomingDto> incommingEntities, Expression<Func<Assessment, bool>> filterPredicate)
        {
            var incomingList = incommingEntities.ToList();
            if (incomingList.Count == 0)
            {
                return;
            }

            var entities = await GetByIdsAsync(incomingList.Select(e => e.Id), filterPredicate: filterPredicate);
            foreach (var entity in entities)
            {
                var incomingEntity = incomingList.FirstOrDefault(x => x.Id == entity.Id);
                if (incomingEntity == null)
                {
                    continue;
                }

                entity.Name = incomingEntity.Name;
            }
            await DbContext.SaveChangesAsync();

        }

    }
}
