using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Application.Mapping;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Services
{
    public class AssessmentService : IAssessmentService
    {
        private readonly IAssessmentRepository _assessmentRepository;
        public AssessmentService(IAssessmentRepository assessmentRepository)
        {
            _assessmentRepository = assessmentRepository;
        }
        public async Task<List<AssessmentOutgoingDto>?> GetAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default)
        {
            var entities = await _assessmentRepository.GetByIdsAsync(ids, withTracking: false, filterPredicate: UserFilter(user), ct: ct);
            if (entities == null || !entities.Any())
            {
                return null;
            }

            return entities.ToOutgoingDtos();
        }
        public async Task<List<AssessmentOutgoingDto>?> CreateAsync(List<AssessmentIncomingDto> dtos, UserOutgoingDto user, CancellationToken ct = default)
        {
            var assessment = dtos.ToEntities(user);
            var entities = await _assessmentRepository.AddRangeAsync(assessment, ct: ct);
            if (entities == null)
            {
                return null;
            }
            return entities.ToOutgoingDtos();
        }
        public async Task DeleteAsync(Guid id, UserOutgoingDto user, CancellationToken ct = default)
        {
            await _assessmentRepository.DeleteByIdsAsync(new List<Guid> { id }, filterPredicate: UserFilter(user), ct: ct);
        }

        public async Task<List<AssessmentOutgoingDto>> GetAllAsync(UserOutgoingDto user, CancellationToken ct = default)
        {
            var entities = await _assessmentRepository.GetAllAsync(withTracking: false, filterPredicate: UserFilter(user), ct: ct);
            return entities.ToOutgoingDtos();
        }

        public async Task UpdateRangeAsync(List<AssessmentIncomingDto> dtos, UserOutgoingDto userDto, CancellationToken ct = default)
        {

            var entities = dtos.ToEntities(userDto);
            await _assessmentRepository.UpdateAsync(entities, filterPredicate: UserFilter(userDto), ct: ct);
        }
        private static Expression<Func<Assessment, bool>> UserFilter(UserOutgoingDto user)
        => e => e!.Project!.ProjectRoles.Any(p => p.UserId == user.Id);
    }
}
