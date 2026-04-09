using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Application.Mapping;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Services
{
    public class DecisionQualityAssessmentService : IDecisionQualityAssessmentService
    {
        private readonly IDecisionQualityAssessmentRepository _DecisionQualityAssessmentRepository;

        public DecisionQualityAssessmentService(IDecisionQualityAssessmentRepository DecisionQualityAssessmentRepository)
        {
            _DecisionQualityAssessmentRepository = DecisionQualityAssessmentRepository;

        }

        public async Task<List<DecisionQualityAssessmentOutgoingDto>> GetAllAsync(UserOutgoingDto user, CancellationToken ct = default)
        {
            var entities = await _DecisionQualityAssessmentRepository.GetAllAsync(withTracking: false, filterPredicate: GetUserFilter(user), ct: ct);
            return entities.ToOutgoingDtos();
        }
        public async Task<DecisionQualityAssessmentOutgoingDto> GetAsync(Guid id, UserOutgoingDto user, CancellationToken ct = default)
        {
            var entity = await _DecisionQualityAssessmentRepository.GetByIdAsync(id, withTracking: false, filterPredicate: GetUserFilter(user), ct: ct);
            if (entity == null)
            {
                throw new KeyNotFoundException($"DecisionQualityAssessment with id {id} not found.");
            }
            return entity.ToOutgoingDto();
        }


        public async Task<List<DecisionQualityAssessmentOutgoingDto>> CreateAsync(List<DecisionQualityAssessmentIncomingDto> dtos, UserOutgoingDto user, CancellationToken ct = default)
        {
            var entities = dtos.ToEntities(user);

            foreach (var entity in entities.Where(x => x.Id == Guid.Empty))
            {
                entity.Id = Guid.NewGuid();
            }

            await _DecisionQualityAssessmentRepository.AddRangeAsync(entities);
            return entities.ToOutgoingDtos();
        }

        public async Task UpdateAsync(List<DecisionQualityAssessmentIncomingDto> dtos, UserOutgoingDto userDto, CancellationToken ct = default)
        {
            var entities = dtos.ToEntities(userDto);
            await _DecisionQualityAssessmentRepository.UpdateRangeAsync(entities, filterPredicate: UserFilter(userDto), ct: ct);
        }

        public async Task DeleteAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default)
        {
            await _DecisionQualityAssessmentRepository.DeleteByIdsAsync(ids, filterPredicate: UserFilter(user), ct: ct);
        }
        private static Expression<Func<DecisionQualityAssessment, bool>> GetUserFilter(UserOutgoingDto user)
        => e => e!.Assessment!.Project!.ProjectRoles.Any(p => p.UserId == user.Id);

        private static Expression<Func<DecisionQualityAssessment, bool>> UserFilter(UserOutgoingDto user)
        => e => e!.Assessment!.Project!.ProjectRoles.Any(p => p.UserId == user.Id && p.Role == ProjectRoleType.Facilitator.ToString());
    }
}
