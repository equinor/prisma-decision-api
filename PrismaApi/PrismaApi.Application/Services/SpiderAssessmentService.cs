using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Application.Mapping;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Services
{
    public class SpiderAssessmentService : ISpiderAssessmentService
    {
        private readonly ISpiderAssessmentRepository _spiderAssessmentRepository;

        public SpiderAssessmentService(ISpiderAssessmentRepository spiderAssessmentRepository)
        {
            _spiderAssessmentRepository = spiderAssessmentRepository;

        }

        public async Task<List<SpiderAssessmentOutgoingDto>> GetAllAsync(UserOutgoingDto user)
        {
            var entities = await _spiderAssessmentRepository.GetAllAsync(withTracking: false, filterPredicate: UserFilter(user));
            return entities.ToOutgoingDtos();
        }
        public async Task<SpiderAssessmentOutgoingDto> GetAsync(Guid id, UserOutgoingDto user)
        {
            var entity = await _spiderAssessmentRepository.GetByIdAsync(id, withTracking: false, filterPredicate: UserFilter(user));
            if (entity == null)
            {
                throw new KeyNotFoundException($"SpiderAssessment with id {id} not found.");
            }
            return entity.ToOutgoingDto();
        }


        public async Task<List<SpiderAssessmentOutgoingDto>> CreateAsync(List<SpiderAssessmentIncomingDto> dtos, UserOutgoingDto user)
        {
            var entities = dtos.ToEntities(user);

            foreach (var entity in entities.Where(x => x.Id == Guid.Empty))
            {
                entity.Id = Guid.NewGuid();
            }

            await _spiderAssessmentRepository.AddRangeAsync(entities);
            return entities.ToOutgoingDtos();
        }

        public async Task UpdateAsync(List<SpiderAssessmentIncomingDto> dtos, UserOutgoingDto user)
        {
            await _spiderAssessmentRepository.UpdateRangeAsync(dtos, filterPredicate: UserFilter(user));
        }

        public async Task DeleteAsync(List<Guid> ids, UserOutgoingDto user)
        {
            var existing = await _spiderAssessmentRepository.GetAllAsync(withTracking: true, filterPredicate: x => ids.Contains(x.Id) && UserFilter(user).Compile()(x));

            await _spiderAssessmentRepository.DeleteByIdsAsync(existing.Select(x => x.Id), filterPredicate: UserFilter(user));

        }
        private static Expression<Func<SpiderAssessment, bool>> UserFilter(UserOutgoingDto user)
        => e => e!.Assessment!.Project!.ProjectRoles.Any(p => p.UserId == user.Id);
    }
}
