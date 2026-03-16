using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Interfaces.Repositories
{
    public interface IAssessmentRepository : ICrudRepository<Assessment, Guid>
    {
        Task UpdateAsync(List<AssessmentIncomingDto> incomingEntities, Expression<Func<Assessment, bool>> filterPredicate);

    }
}
