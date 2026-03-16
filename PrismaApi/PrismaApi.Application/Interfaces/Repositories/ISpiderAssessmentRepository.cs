using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Interfaces.Repositories
{
    public interface ISpiderAssessmentRepository : ICrudRepository<SpiderAssessment, Guid>

    {
        Task UpdateRangeAsync(List<SpiderAssessmentIncomingDto> incommingEntities, Expression<Func<SpiderAssessment, bool>> filterPredicate);
    }
}
