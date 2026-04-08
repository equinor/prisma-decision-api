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

        public async Task<List<SpiderAssessmentOutgoingDto>> GetAllAsync(UserOutgoingDto user, CancellationToken ct = default)
        {
            var entities = await _spiderAssessmentRepository.GetAllAsync(withTracking: false, filterPredicate: UserFilter(user), ct: ct);
            return entities.ToOutgoingDtos();
        }
        public async Task<SpiderAssessmentOutgoingDto> GetAsync(Guid id, UserOutgoingDto user, CancellationToken ct = default)
        {
            var entity = await _spiderAssessmentRepository.GetByIdAsync(id, withTracking: false, filterPredicate: UserFilter(user), ct: ct);
            if (entity == null)
            {
                throw new KeyNotFoundException($"SpiderAssessment with id {id} not found.");
            }
            return entity.ToOutgoingDto();
        }


        public async Task<List<SpiderAssessmentOutgoingDto>> CreateAsync(List<SpiderAssessmentIncomingDto> dtos, UserOutgoingDto user, CancellationToken ct = default)
        {
            var entities = dtos.ToEntities(user);

            foreach (var entity in entities.Where(x => x.Id == Guid.Empty))
            {
                entity.Id = Guid.NewGuid();
            }

            await _spiderAssessmentRepository.AddRangeAsync(entities);
            return entities.ToOutgoingDtos();
        }

        public async Task UpdateAsync(List<SpiderAssessmentIncomingDto> dtos, UserOutgoingDto userDto, CancellationToken ct = default)
        {
            var entities = dtos.ToEntities(userDto);
            await _spiderAssessmentRepository.UpdateRangeAsync(entities, filterPredicate: UserFilter(userDto), ct: ct);
        }

        public async Task DeleteAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default)
        {
            await _spiderAssessmentRepository.DeleteByIdsAsync(ids, filterPredicate: UserFilter(user), ct: ct);
        }
        private static Expression<Func<SpiderAssessment, bool>> UserFilter(UserOutgoingDto user)
        => e => e!.Assessment!.Project!.ProjectRoles.Any(p => p.UserId == user.Id);
    }
}
